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

```csharp
private static ManualResetEvent connectDone = new ManualResetEvent(false);
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
