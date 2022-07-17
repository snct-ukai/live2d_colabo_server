using System.Net.Sockets;
using System.Net;

namespace live2d_chat_server
{
  enum socket_flag{
    undefined = 0,
    register = 1,
    client_tracking_data = 2,
    member_tracking_data = 3,
    join_room = 4,
    create_room = 5,
    leave_room = 6,
    remove_room = 7,
    ack = 8,
    ready = 9,
    client_not_connect = 10,
    reconnect = 11,
  }

  class UdpState{
    public UdpClient udpClient;
    public IPEndPoint endPoint;

    public UdpState(UdpClient udpClient, IPEndPoint endPoint){
      this.udpClient = udpClient;
      this.endPoint = endPoint;
    }
  }

  class Server{
    private Dictionary<IPEndPoint, int> clients = new Dictionary<IPEndPoint, int>();
    private Dictionary<int, List<IPEndPoint>> rooms = new Dictionary<int, List<IPEndPoint>>();
    private string local;
    private IPAddress ip = IPAddress.Any;
    private int port;
    private IPEndPoint? ipe;
    private UdpClient? server_socket;

    private struct Flag{
      public int sendFlag;
      public int recvFlag;

      public Flag(int sendFlag, int recvFlag){
        this.sendFlag = sendFlag;
        this.recvFlag = recvFlag;
      }
    }

    private struct ID{
      public int userID;
      public int roomID;

      public ID(int userID, int roomID){
        this.userID = userID;
        this.roomID = roomID;
      }
    }
    
    //singleton
    private static Server instance = new Server();

    public static Server getInstance(){
      return instance;
    }
  
    private Server(string local = "127.0.0.1", int port = 9000){
      this.local = local;
      this.port = port;
    }
  
    public void start(string local = "127.0.0.1", int port = 9000){
      this.local = local;
      this.port = port;
      this.ip = IPAddress.Parse(this.local);
      this.ipe = new IPEndPoint(this.ip, this.port);
      if(this.server_socket != null){
        return;
      }
      else{
        this.server_socket = new UdpClient(this.ipe);
        for(;;){
          this.server_socket.BeginReceive(this.OnReceive, new UdpState(this.server_socket, this.ipe));
        }
      }
    }

    private void OnReceive(IAsyncResult ar){
      UdpState? udpState = ar.AsyncState as UdpState;
      if(udpState != null){
        UdpClient udpClient = udpState.udpClient;
        IPEndPoint? endPoint = udpState.endPoint;
        byte[] message = udpClient.EndReceive(ar, ref endPoint);
      }
    }

    private Flag GetFlag(byte[] message){
      byte flag = message[0];
      byte recvflag_mask = 0x0f;
      byte sendflag_mask = 0xf0;
      byte sendflag = (byte)((flag & sendflag_mask) >> 4);
      byte recvflag = (byte)(flag & recvflag_mask);
      return new Flag(sendflag, recvflag);
    }

    private ID GetID(byte[] message){
      byte ID = message[1];
      byte roomID_mask = 0x0f;
      byte userID_mask = 0xf0;
      byte userID = (byte)((ID & userID_mask) >> 4);
      byte roomID = (byte)(ID & roomID_mask);
      return new ID(userID, roomID);
    }

    private void send(IPEndPoint endPoint, byte[] message){
      if(this.server_socket == null){
        this.ipe = new IPEndPoint(this.ip, this.port);
        server_socket = new UdpClient(this.ipe);
      }
      this.server_socket.SendAsync(message, message.Length, endPoint);
    }
  
    private void stop(){
      if(this.server_socket != null){
        foreach(var room in this.rooms){
          foreach(var client in room.Value){
            byte flag = (byte)socket_flag.leave_room;
            flag += (byte)(room.Key << 4);
            byte userID = 0;
            byte roomID = (byte)(room.Key);
            byte[] message = new byte[3]{flag, userID, roomID};
            this.send(client, message);
          }
        }
        this.server_socket.Close();
        rooms.Clear();
      }
    }

    //room process
    private void createRoom(IPEndPoint endPoint, int id){
      if(this.rooms.ContainsKey(id)){
        return;
      }
      else{
        this.rooms.Add(id, new List<IPEndPoint>());
        this.rooms[id].Add(endPoint);
        byte flag = (byte)socket_flag.create_room;
        flag += (byte)(id << 4);
        byte roomID = (byte)id;
        byte[] message = new byte[2]{flag, roomID};
        this.send(endPoint, message);
      }
    }

    private void joinRoom(IPEndPoint endPoint, int id){
      if(this.rooms.ContainsKey(id)){
        this.rooms[id].Add(endPoint);
        byte flag = (byte)socket_flag.join_room;
        flag += (byte)(id << 4);
        byte roomID = (byte)id;
        byte[] message = new byte[2]{flag, roomID};
        this.send(endPoint, message);
      }
    }

    private void leaveRoom(IPEndPoint endPoint, int id){
      if(this.rooms.ContainsKey(id)){
        this.rooms[id].Remove(endPoint);
        if(this.rooms[id].Count == 0){
          this.rooms.Remove(id);
        }
        byte flag = (byte)socket_flag.leave_room;
        flag += (byte)(id << 4);
        byte roomID = (byte)id;
        byte[] message = new byte[2]{flag, roomID};
        this.send(endPoint, message);
      }
    }

    private void removeRoom(int id){
      if(this.rooms.ContainsKey(id)){
        foreach(var client in this.rooms[id]){
          byte flag = (byte)socket_flag.remove_room;
          flag += (byte)(id << 4);
          byte roomID = (byte)id;
          byte[] message = new byte[2]{flag, roomID};
          this.send(client, message);
        }
        this.rooms.Remove(id);
      }
    }

    //send tracking data
    private void sendTrackingData(int id, byte[] message){
      if(this.rooms.ContainsKey(id)){
        foreach(var client in this.rooms[id]){
          this.send(client, message);
        }
      }
    }

    //ACK
    private void sendACK(IPEndPoint endPoint, byte recvflag){
      byte id = 0;
      byte flag = (byte)(((byte)socket_flag.ack << 4) + recvflag);
      byte roomID = (byte)id;
      byte userid = (byte)clients[endPoint];
      byte[] message = new byte[3]{flag, userid, roomID};
      this.send(endPoint, message);
    }
  }
}
