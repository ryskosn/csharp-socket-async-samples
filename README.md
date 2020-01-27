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

- [ManualResetEvent クラス (System.Threading) | Microsoft Docs](https://docs.microsoft.com/ja-jp/dotnet/api/system.threading.manualresetevent)
- [同期プリミティブの概要 | Microsoft Docs](https://docs.microsoft.com/ja-jp/dotnet/standard/threading/overview-of-synchronization-primitives)

継承元は `WaitHandle` クラス。

- [WaitHandle クラス (System.Threading) | Microsoft Docs](https://docs.microsoft.com/ja-jp/dotnet/api/system.threading.waithandle)

コストラクタに `false` を渡すことで非シグナル状態にする。

今回のコードでは server では 1 つ、client では 3 つ作成している。

```csharp : Server/Program.cs
// Server
public static ManualResetEvent allDone = new ManualResetEvent(false);
```

```csharp :Client/Program.cs
// Client
private static ManualResetEvent connectDone = new ManualResetEvent(false);
private static ManualResetEvent sendDone = new ManualResetEvent(false);
private static ManualResetEvent receiveDone = new ManualResetEvent(false);
```

今回のコードで出てくるメソッドは `WaitOne()`、`Set()`、`Reset()`。

#### `WaitOne()`

> 現在の WaitHandle がシグナルを受け取るまで、現在のスレッドをブロックします。  
> [WaitHandle.WaitOne メソッド](https://docs.microsoft.com/ja-jp/dotnet/api/system.threading.waithandle.waitone#System_Threading_WaitHandle_WaitOne)

#### `Set()`

> イベントの状態をシグナル状態に設定し、待機している 1 つ以上のスレッドが進行できるようにします。  
> [EventWaitHandle.Set メソッド](https://docs.microsoft.com/ja-jp/dotnet/api/system.threading.eventwaithandle.set)

#### `Reset()`

> イベントの状態を非シグナル状態に設定し、スレッドをブロックします。  
> [EventWaitHandle.Reset メソッド](https://docs.microsoft.com/ja-jp/dotnet/api/system.threading.eventwaithandle.reset#System_Threading_EventWaitHandle_Reset)

### `BeginAccept`, `EndAccept`

```csharp
listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
```

```csharp
Socket Socket.EndAccept(IAsyncResult asyncResult)
```

> Asynchronously accepts an incoming connection attempt and creates a new System.Net.Sockets.Socket to handle remote host communication.

- [Socket.BeginAccept メソッド (System.Net.Sockets) | Microsoft Docs](https://docs.microsoft.com/ja-jp/dotnet/api/system.net.sockets.socket.beginaccept#System_Net_Sockets_Socket_BeginAccept_System_AsyncCallback_System_Object_)
- [Socket.EndAccept メソッド (System.Net.Sockets) | Microsoft Docs](https://docs.microsoft.com/ja-jp/dotnet/api/system.net.sockets.socket.endaccept#System_Net_Sockets_Socket_EndAccept_System_IAsyncResult_)
- [IAsyncResult インターフェイス (System) | Microsoft Docs](https://docs.microsoft.com/ja-jp/dotnet/api/system.iasyncresult)

---

1. `StartListening()` の中で socket を用意して、Endpoint (= ip address & port) に `Bind` して `Listen` する。

2. そして [`BeginAccept()`](https://docs.microsoft.com/ja-jp/dotnet/api/system.net.sockets.socket.beginaccept) (with `AcceptCallback`) を呼び、client から connect されるのを待つ。

3. connect されると、`AcceptCallback()` が [`IAsyncResult`](https://docs.microsoft.com/ja-jp/dotnet/api/system.iasyncresult) `ar` と共に呼ばれる。

   1. `IAsyncResult ar` のメンバー（プロパティ）である [`AsyncState`](https://docs.microsoft.com/ja-jp/dotnet/api/system.iasyncresult.asyncstate) という object をキャストして `listener` という `Socket` にする。

   2. `listener` が [`EndAccept()`](https://docs.microsoft.com/ja-jp/dotnet/api/system.net.sockets.socket.endaccept) を呼び、client からの接続を handle するための新しい `Socket`、`handler` を作り出す。

   3. `handler` が `BeginReceive()` (with `ReadCallback`) を呼び、 client から送られてくるデータを受信するため、待機を開始する。

4. データの受信が始まると、`ReadCallback()` が `IAsyncResult ar` と共に呼ばれる。

   1. `IAsyncResult ar` のメンバーである `AsyncState` という object をキャストして `state` という `StateObject` （これは自分で定義したもの）にする。

   2. `StateObject state` がメンバーとして持つ `Socket` を `handler` という変数に代入する。

   3. `handler` が `EndReceive()` を `IAsyncResult ar` と共に呼ぶ。

   4. `state` が持つ `byte[] buffer` の中身を encode して `StringBuilder` 型である `state.sb` に入れ、それを `string` にして変数 `content` に入れる。
      1. 終端文字をチェックして読み込み終了、もしくは、`handler` が再び `BeginReceive()` を呼ぶ。
      2. 終端文字があった場合、`content` の中身を console に表示し、`Send()` を `Socket handler` と `string content` と共に呼ぶ。

5. `Send()` では `content` を `byte[]` に encode して、 `BeginSend()` (with `SendCallback`) を呼ぶ。

---

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
