using Microsoft.AspNetCore.SignalR.Client;
using NetSupport.Shared.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetSupport.Student.Services
{
    public class StudentClient
    {
        private HubConnection _connection;
        private StudentInfo _currentStudent;
        private HeartbeatService _heartbeatService;

        public event Action<string> OnStatusChanged;
        public bool IsConnected => _connection?.State == HubConnectionState.Connected;

        public async Task ConnectAsync(string url, StudentInfo student)
        {
            _currentStudent = student;
            _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();

            _connection.Reconnecting += (error) => {
                OnStatusChanged?.Invoke("Reconnecting...");
                return Task.CompletedTask;
            };
            _connection.Reconnected += (connectionId) => {
                OnStatusChanged?.Invoke("Connected");
                return Task.CompletedTask;
            };

            await _connection.StartAsync();

            await _connection.InvokeAsync("RegisterStudent", _currentStudent);

            OnStatusChanged?.Invoke("Connected");
            _heartbeatService = new HeartbeatService(async () =>
            {
                if (IsConnected && _currentStudent != null)
                {
                    try
                    {
                        await _connection.InvokeAsync("SendHeartbeat", _currentStudent);
                    }
                    catch {}
                }
            });
            _heartbeatService.Start();
        }

        public async Task StopAsync()
        {
            _heartbeatService?.Stop(); 
            if (_connection != null)
            {
                await _connection.StopAsync();
            }
        }
    }
}    