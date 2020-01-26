## Socket 非同期通信サンプル

- [非同期サーバー ソケットの例 | Microsoft Docs](https://docs.microsoft.com/ja-jp/dotnet/framework/network-programming/asynchronous-server-socket-example)
- [非同期クライアント ソケットの例 | Microsoft Docs](https://docs.microsoft.com/ja-jp/dotnet/framework/network-programming/asynchronous-client-socket-example)

### 実行

```sh
$ cd SocketSample
$ dotnet run --project Server/
```

```sh
# another console
$ cd SocketSample
$ dotnet run --project Client/
```

## 理解メモ

### `ManualResetEvent`

- [ManualResetEvent クラス (System.Threading) | Microsoft Docs](https://docs.microsoft.com/ja-jp/dotnet/api/system.threading.manualresetevent?view=netframework-4.8)
- [同期プリミティブの概要 | Microsoft Docs](https://docs.microsoft.com/ja-jp/dotnet/standard/threading/overview-of-synchronization-primitives?view=netframework-4.8)

継承元は `WaitHandle` クラス。

- [WaitHandle クラス (System.Threading) | Microsoft Docs](https://docs.microsoft.com/ja-jp/dotnet/api/system.threading.waithandle?view=netframework-4.8)

コストラクタに `false` を渡すことで非シグナル状態にする。

今回のコードでは server では 1 つ、client では 3 つ作成している。

```csharp
// Server
public static ManualResetEvent allDone = new ManualResetEvent(false);
```

```csharp
// Client
private static ManualResetEvent connectDone = new ManualResetEvent(false);
private static ManualResetEvent sendDone = new ManualResetEvent(false);
private static ManualResetEvent receiveDone = new ManualResetEvent(false);
```

今回のコードで出てくるメソッドは `WaitOne()`、`Set()`、`Reset()`。

#### `WaitOne()`

> 現在の WaitHandle がシグナルを受け取るまで、現在のスレッドをブロックします。  
> [WaitHandle.WaitOne メソッド](https://docs.microsoft.com/ja-jp/dotnet/api/system.threading.waithandle.waitone?view=netframework-4.8#System_Threading_WaitHandle_WaitOne)

#### `Set()`

> イベントの状態をシグナル状態に設定し、待機している 1 つ以上のスレッドが進行できるようにします。  
> [EventWaitHandle.Set メソッド](https://docs.microsoft.com/ja-jp/dotnet/api/system.threading.eventwaithandle.set?view=netframework-4.8)

#### `Reset()`

> イベントの状態を非シグナル状態に設定し、スレッドをブロックします。  
> [EventWaitHandle.Reset メソッド](https://docs.microsoft.com/ja-jp/dotnet/api/system.threading.eventwaithandle.reset?view=netframework-4.8#System_Threading_EventWaitHandle_Reset)

### `BeginAccept`, `EndAccept`

```csharp
listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
```

```csharp
Socket Socket.EndAccept(IAsyncResult asyncResult)
```

> Asynchronously accepts an incoming connection attempt and creates a new System.Net.Sockets.Socket to handle remote host communication.

- [Socket.BeginAccept メソッド (System.Net.Sockets) | Microsoft Docs](https://docs.microsoft.com/ja-jp/dotnet/api/system.net.sockets.socket.beginaccept?view=netframework-4.8#System_Net_Sockets_Socket_BeginAccept_System_AsyncCallback_System_Object_)
- [Socket.EndAccept メソッド (System.Net.Sockets) | Microsoft Docs](https://docs.microsoft.com/ja-jp/dotnet/api/system.net.sockets.socket.endaccept?view=netframework-4.8#System_Net_Sockets_Socket_EndAccept_System_IAsyncResult_)
- [IAsyncResult インターフェイス (System) | Microsoft Docs](https://docs.microsoft.com/ja-jp/dotnet/api/system.iasyncresult?view=netframework-4.8)

### `SendCallback()`

server は `Shutdown()` と `Close()` を呼んでいるが、

```csharp
// Server

// Complete sending the data to the remote device.
int bytesSent = handler.EndSend(ar);
Console.WriteLine("Sent {0} bytes to client.", bytesSent);

handler.Shutdown(SocketShutdown.Both);
handler.Close();
```

client の方は `Socket` に対しては何もせず、`ManualResetEvent` の `Set()` を呼んでいる。  
（＝ `sendDone` をシグナル状態に変更している）

```csharp
// Client

// Complete sending the data to the remote device.
int bytesSent = client.EndSend(ar);
Console.WriteLine("Sent {0} bytes to server.", bytesSent);

// Signal that all bytes have been sent.
sendDone.Set();
```

これが「server は終了せず待機し続ける一方、client は終了する」という動作になっている原因だろうか？
