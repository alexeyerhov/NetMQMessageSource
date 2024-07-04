
using NetMQ;
using NetMQ.Sockets;
using NetMQMessageSource;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetMQMessageSource
{
    /// <summary>
    /// Реализация IMessageSource с использованием NetMQ для отправки сообщений.
    /// </summary>
    public class NetMQMessageSource : IMessageSource
    {
        private readonly string _address;
        private readonly ResponseSocket _socket;

        /// <summary>
        /// Конструктор для NetMQMessageSource.
        /// </summary>
        /// <param name="address">Адрес для подключения NetMQ.</param>
        public NetMQMessageSource(string address)
        {
            _address = address;
            _socket = new ResponseSocket();
            _socket.Bind(_address);
        }

        /// <summary>
        /// Отправка сообщения через NetMQ.
        /// </summary>
        /// <param name="message">Сообщение для отправки.</param>
        public void SendMessage(string message)
        {
            _socket.SendFrame(message);
        }

        /// <summary>
        /// Завершение работы сокета.
        /// </summary>
        public void Dispose()
        {
            _socket.Dispose();
        }
    }

    /// <summary>
    /// Реализация IMessageSourceClient с использованием NetMQ для получения сообщений.
    /// </summary>
    public class NetMQMessageSourceClient : IMessageSourceClient
    {
        private readonly string _address;
        private readonly RequestSocket _socket;
        private readonly CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Конструктор для NetMQMessageSourceClient.
        /// </summary>
        /// <param name="address">Адрес для подключения NetMQ.</param>
        public NetMQMessageSourceClient(string address)
        {
            _address = address;
            _socket = new RequestSocket();
            _socket.Connect(_address);
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Получение сообщений через NetMQ.
        /// </summary>
        /// <returns>Асинхронный поток с полученными сообщениями.</returns>
        public async Task<string> ReceiveMessageAsync()
        {
            return await _socket.ReceiveFrameStringAsync(_cancellationTokenSource.Token);
        }

        /// <summary>
        /// Отправка сообщения через NetMQ.
        /// </summary>
        /// <param name="message">Сообщение для отправки.</param>
        public void SendMessage(string message)
        {
            _socket.SendFrame(message);
        }

        /// <summary>
        /// Завершение работы сокета.
        /// </summary>
        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _socket.Dispose();
        }
    }
}

/// Сервер

using var messageSource = new NetMQMessageSource("tcp://*:5555");
Console.WriteLine("Сервер запущен. Введите сообщение для отправки:");
while (true)
{
    var message = Console.ReadLine();
    if (message == "quit") break;
    messageSource.SendMessage(message);
}
    ```

/// Клиент

using var messageSourceClient = new NetMQMessageSourceClient("tcp://localhost:5555");
Console.WriteLine("Клиент запущен. Введите сообщение для отправки:");
while (true)
{
    var message = Console.ReadLine();
    if (message == "quit") break;
    messageSourceClient.SendMessage(message);
    Console.WriteLine($"Получено сообщение: {await messageSourceClient.ReceiveMessageAsync()}");
}

