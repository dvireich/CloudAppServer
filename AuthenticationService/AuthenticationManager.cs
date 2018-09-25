using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBManager;
using DBManager.Interfaces;
using ServiceLoadTaskQueue;

namespace AuthenticationService
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly IUserRepositoryFactory _userRepositoryFactory;
        private readonly ITaskQueue _taskQueue;
        private readonly IConnectecClients _connectecClients;

        public AuthenticationManager(
            IUserRepositoryFactory userRepositoryFactory, 
            ITaskQueue taskQueue,
            IConnectecClients connectecClients)
        {
            _userRepositoryFactory = userRepositoryFactory;
            _taskQueue = taskQueue;
            _connectecClients = connectecClients;
        }

        public AuthenticationManager()
        {
            _taskQueue = TaskQueue.Instance;
            _userRepositoryFactory = new UserRepositoryFactory();
            _connectecClients = ConnectecClients.Instance;
        }

        public string Authenticate(string userName, string password)
        {
            var userRepository = _userRepositoryFactory.GetUserRepository();
            if (!userRepository.CheckUserNameAndPassword(userName, password, out var user))
            {
                throw new Exception("User name or password is incorrect!");
            }

            if (_connectecClients.Contains(user.Id))
            {
                throw new Exception("You already connected!. please logout and try again...");
            }

            _connectecClients.Add(user.Id);
            _taskQueue.AddToTaskQueue(new UserData()
            {
                Id = user.Id,
                OnRemove = () =>
                {
                    _connectecClients.Remove(user.Id);
                }
            });
            return user.Id;
        }

        public void Register(string userName, string password)
        {
            var userRepository = _userRepositoryFactory.GetUserRepository();
            if (userRepository.CheckUserNameExists(userName))
            {
                throw new Exception($"The user name: {userName} is taken. Try another one");
            }

            userRepository.SaveUserInDb(new User()
            {
                Id = Guid.NewGuid().ToString(),
                Password = password,
                UserName = userName
            });
        }

        public bool IsUserNameTaken(string userName)
        {
            var userRepository = _userRepositoryFactory.GetUserRepository();
            return userRepository.CheckUserNameExists(userName);
        }
    }
}
