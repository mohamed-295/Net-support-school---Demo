using Microsoft.AspNetCore.SignalR.Client;
using NetSupport.Shared.Models;
using NetSupport.Shared.Contracts;
using NetSupport.Student.Forms;
using System.Linq;
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

        private LockScreenForm _lockScreenForm;
        private string _activeSessionId = string.Empty;

        public event Action<string> OnStatusChanged;
        public bool IsConnected => _connection?.State == HubConnectionState.Connected;

        private TestTakingForm _activeTestForm;
        public static HubConnection Connection { get; private set; }

        public async Task ConnectAsync(string url, StudentInfo student)
        {
            _currentStudent = student;
            _connection = new HubConnectionBuilder()
                .WithUrl(url)
                .WithAutomaticReconnect()
                .Build();


            Connection = _connection;    

            _connection.Reconnecting += (error) => {
                OnStatusChanged?.Invoke("Reconnecting...");
                return Task.CompletedTask;
            };
            _connection.Reconnected += (connectionId) => {
                OnStatusChanged?.Invoke("Connected");
                return Task.CompletedTask;
            };

            await _connection.StartAsync();
            RegisterTestHandlers();

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

        private void RegisterTestHandlers()
        {
    // Start Test
        _connection.On<TutorCommand>("ReceiveCommand", (command) =>
        {
            var hostForm = Application.OpenForms.Cast<Form>().FirstOrDefault();
            hostForm?.Invoke(new Action(async () =>
            {
                switch (command.CommandType)
                {
                    case "Lock":
                        ShowLockScreen();
                        break;
                    case "Unlock":
                        HideLockScreen();
                        break;
                    case "StartTest":
                        await HandleStartTestAsync(command);
                        break;
                    case "StopTest":
                        HandleStopTest();
                        break;
                }
            }));
        });
}

    private void ShowLockScreen()
    {
        if (_lockScreenForm != null && !_lockScreenForm.IsDisposed)
        {
            return;
        }

        _lockScreenForm = new LockScreenForm();
        _lockScreenForm.Show();
    }

    private void HideLockScreen()
    {
        if (_lockScreenForm == null)
        {
            return;
        }

        _lockScreenForm.Close();
        _lockScreenForm = null;
    }

    private async Task HandleStartTestAsync(TutorCommand command)
    {
        _activeSessionId = command.SessionId ?? command.Exam?.Id ?? string.Empty;
        var login = new TestLoginForm(_currentStudent.FullName);
        if (login.ShowDialog() == DialogResult.OK)
        {
            _activeTestForm = new TestTakingForm(command.Exam!, _currentStudent.StudentId, _activeSessionId);
            _activeTestForm.Show();
        }
    }

    private void HandleStopTest()
    {
        _activeTestForm?.SubmitExam();
        _activeTestForm = null;
    }
    }
}    