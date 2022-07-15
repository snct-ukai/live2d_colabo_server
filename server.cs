using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace live2d_chat_server
{
  enum socket_flag{
    register = 1,
    client_tracking_data = 2,
    member_tracking_data = 3,
    join_room = 4,
    create_room = 5,
    leave_room = 6,
    remove_room = 7,
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
    private Dictionary<IPEndPoint, string> clients = new Dictionary<IPEndPoint, string>();
    private Dictionary<string, List<IPEndPoint>> rooms = new Dictionary<string, List<IPEndPoint>>();
    private string local = "127.0.0.1";
    private IPAddress ip = IPAddress.Any;
    private int port = 9000;
    private IPEndPoint? ipe;
    private UdpClient? server_socket;
    
    //singleton
    private static Server instance = new Server();

    public static Server getInstance(){
      return instance;
    }
  
    private Server(){}
  
    public void start(string? local, int? port){
      this.local = local ?? this.local;
      this.port = port ?? this.port;
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
      if((UdpState)ar.AsyncState != null){
        UdpClient udpClient = ((UdpState)ar.AsyncState).udpClient;
        IPEndPoint endPoint = ((UdpState)ar.AsyncState).endPoint;
        byte[] message = udpClient.EndReceive(ar, ref endPoint);
      }
    }
  
    public void stop(){
      if(this.server_socket != null){
        foreach(var room in this.rooms){
          foreach(var client in room.Value){
            Int32 flag = (Int32)socket_flag.leave_room;
            byte[] f = BitConverter.GetBytes(flag);
            byte[] key = Encoding.UTF8.GetBytes(room.Key);
            byte[] data = f.Concat(key).ToArray();
            server_socket.SendAsync(data, client);
          }
        }
        this.server_socket.Close();
        rooms.Clear();
      }
    }
  }
}
