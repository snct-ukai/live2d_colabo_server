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
    client_disconnect = 12,
    TCP_socket_info = 13,
  }

  class UdpState{
    public UdpClient udpClient;
    public IPEndPoint endPoint;

    public UdpState(UdpClient udpClient, IPEndPoint endPoint){
      this.udpClient = udpClient;
      this.endPoint = endPoint;
    }
  }

  class UDP_server{
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
    private static UDP_server instance = new UDP_server();

    public static ref UDP_server getInstance(){
      return ref instance;
    }
  
    private UDP_server(string local = "127.0.0.1", int port = 9000){
      this.local = local;
      this.port = port;
    }
  
    public void start(string local = "127.0.0.1", int port = 9000){
      try{
        this.local = local;
        this.port = port;
        this.ip = IPAddress.Parse(ipString: this.local);
        this.ipe = new IPEndPoint(address: this.ip, port: this.port);
        if(this.server_socket != null){
          return;
        }
        else{
          this.server_socket = new UdpClient(localEP: this.ipe);
          for(;;){
            this.server_socket.BeginReceive(requestCallback: this.OnReceive, state: new UdpState(udpClient: this.server_socket, endPoint: this.ipe));
          }
        }
      }catch{
        Console.WriteLine(value: "UDP_server start error");
        throw new Exception(message: "UDP_server start error");
      }
    }

    private void OnReceive(IAsyncResult ar){
      UdpState? udpState = ar.AsyncState as UdpState;
      if(udpState != null){
        UdpClient udpClient = udpState.udpClient;
        IPEndPoint? endPoint = udpState.endPoint;
        byte[] message = udpClient.EndReceive(asyncResult: ar, remoteEP: ref endPoint);
      }
    }

    private Flag GetFlag(byte[] message){
      byte flag = message[0];
      byte recvflag_mask = 0x0f;
      byte sendflag_mask = 0xf0;
      byte sendflag = (byte)((flag & sendflag_mask) >> 4);
      byte recvflag = (byte)(flag & recvflag_mask);
      return new Flag(sendFlag: sendflag, recvFlag: recvflag);
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
        this.ipe = new IPEndPoint(address: this.ip, port: this.port);
        server_socket = new UdpClient(localEP: this.ipe);
      }
      this.server_socket.SendAsync(datagram: message, bytes: message.Length, endPoint);
    }
  
    private void stop(){
      if(this.server_socket != null){
        foreach(KeyValuePair<int, List<IPEndPoint>> room in this.rooms){
          foreach(IPEndPoint client in room.Value){
            byte flag = (byte)socket_flag.leave_room;
            flag += (byte)(room.Key << 4);
            byte userID = 0;
            byte roomID = (byte)(room.Key);
            byte[] message = new byte[3]{flag, userID, roomID};
            this.send(endPoint: client, message);
          }
        }
        this.server_socket.Close();
        rooms.Clear();
      }
    }

    //room process
    private void createRoom(IPEndPoint endPoint, int id){
      if(this.rooms.ContainsKey(key: id)){
        return;
      }
      else{
        this.rooms.Add(key: id, value: new List<IPEndPoint>());
        this.rooms[key: id].Add(item: endPoint);
        byte flag = (byte)socket_flag.create_room;
        flag += (byte)(id << 4);
        byte roomID = (byte)id;
        byte[] message = new byte[2]{flag, roomID};
        this.send(endPoint, message);
      }
    }

    private void joinRoom(IPEndPoint endPoint, int id){
      if(this.rooms.ContainsKey(key: id)){
        this.rooms[key: id].Add(item: endPoint);
        byte flag = (byte)socket_flag.join_room;
        flag += (byte)(id << 4);
        byte roomID = (byte)id;
        byte[] message = new byte[2]{flag, roomID};
        this.send(endPoint, message);
      }
    }

    private void leaveRoom(IPEndPoint endPoint, int id){
      if(this.rooms.ContainsKey(key: id)){
        this.rooms[key: id].Remove(item: endPoint);
        if(this.rooms[key: id].Count == 0){
          this.rooms.Remove(key: id);
        }
        byte flag = (byte)socket_flag.leave_room;
        flag += (byte)(id << 4);
        byte roomID = (byte)id;
        byte[] message = new byte[2]{flag, roomID};
        this.send(endPoint, message);
      }
    }

    private void removeRoom(int id){
      if(this.rooms.ContainsKey(key: id)){
        foreach(IPEndPoint client in this.rooms[key: id]){
          byte flag = (byte)socket_flag.remove_room;
          flag += (byte)(id << 4);
          byte roomID = (byte)id;
          byte[] message = new byte[2]{flag, roomID};
          this.send(endPoint: client, message);
        }
        this.rooms.Remove(key: id);
      }
    }

    //send tracking data
    private void sendTrackingData(int id, byte[] message){
      if(this.rooms.ContainsKey(key: id)){
        foreach(IPEndPoint client in this.rooms[key: id]){
          this.send(endPoint: client, message);
        }
      }
    }

    //ACK
    private void sendACK(IPEndPoint endPoint, byte recvflag){
      byte id = 0;
      byte flag = (byte)(((byte)socket_flag.ack << 4) + recvflag);
      byte roomID = (byte)id;
      byte userid = (byte)clients[key: endPoint];
      byte[] message = new byte[3]{flag, userid, roomID};
      this.send(endPoint, message);
    }

    public Dictionary<IPEndPoint, int> Getclients(){
      return new Dictionary<IPEndPoint, int>(dictionary: this.clients);
    }

    public Dictionary<int, List<IPEndPoint>> Getrooms(){
      return new Dictionary<int, List<IPEndPoint>>(dictionary: this.rooms);
    }
  }

  //TCP server
  class TCP_server{
    private static string local = string.Empty;
    private static IPAddress ip = IPAddress.Any;
    private static int port;
    private static IPEndPoint? ipe;
    private TcpListener? server_socket;

    //singleton
    private static TCP_server instance = new TCP_server();
    public static ref TCP_server Instance(){
      return ref instance;
    }
    private TCP_server(string local = "127.0.0.1", int port = 9001){
      TCP_server.local = local;
      TCP_server.port = port;
      ip = IPAddress.Parse(ipString: local);
      TCP_server.ipe = new IPEndPoint(address: ip, port);
    }
    
    public void start(string local = "127.0.0.1", int port = 9001){
      try{
        TCP_server.local = local;
        TCP_server.port = port;
        ip = IPAddress.Parse(ipString: local);
        TCP_server.ipe = new IPEndPoint(address: ip, port);
        if(this.server_socket != null){
          this.server_socket.Stop();
        }
        this.server_socket = new TcpListener(localEP: ipe);
        this.server_socket.Server.SetSocketOption(optionLevel: SocketOptionLevel.IP, optionName: SocketOptionName.ReuseAddress, optionValue: true);
        this.server_socket.Start();
      }catch{
        Console.WriteLine(value: "TCP server start failed");
        throw new Exception(message: "TCP server start failed");
      }
    }
  }
}
