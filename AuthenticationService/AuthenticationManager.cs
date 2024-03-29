﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBManager;
using DBManager.Interfaces;
using ServiceLoadTaskQueue;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]


namespace AuthenticationService
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly IUserRepositoryFactory _userRepositoryFactory;
        private readonly ITaskQueue _taskQueue;

        public AuthenticationManager(
            IUserRepositoryFactory userRepositoryFactory, 
            ITaskQueue taskQueue)
        {
            _userRepositoryFactory = userRepositoryFactory;
            _taskQueue = taskQueue;
        }

        public AuthenticationManager()
        {
            _taskQueue = TaskQueue.Instance;
            _userRepositoryFactory = new UserRepositoryFactory();
        }

        public string Authenticate(string userName, string password)
        {
            var userRepository = _userRepositoryFactory.GetUserRepository();
            if (!userRepository.CheckUserNameAndPassword(userName, password, out var user))
            {
                throw new Exception("User name or password is incorrect!");
            }

            _taskQueue.AddToTaskQueue(new UserData()
            {
                Id = user.Id,
            });
            return user.Id;
        }

        public void Register(string userName, string password, string securityAnswer, string securityQuestion)
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
                UserName = userName,
                SecurityAnswer = securityAnswer,
                SecurityQuestion = securityQuestion
            });
        }

        public string GetSecurityQuestion(string userName)
        {
            var userRepository = _userRepositoryFactory.GetUserRepository();
            if (!userRepository.CheckUserNameExists(userName))
            {
                throw new Exception($"The user name: {userName} does not exists!");
            }

            var user = userRepository.GetUserByUserName(userName);
            return user.SecurityQuestion;
        }

        public string RestorePassword(string userName, string securityAnswer)
        {
            var userRepository = _userRepositoryFactory.GetUserRepository();
            if (!userRepository.CheckUserNameExists(userName))
            {
                throw new Exception($"The user name: {userName} does not exists!");
            }
            var user = userRepository.GetUserByUserName(userName);
            if (user.SecurityAnswer.ToLower() != securityAnswer.ToLower())
            {
                throw new Exception($"The answer: {securityAnswer} does not match!");
            }

            return user.Password;
        }
        

        public bool IsUserNameTaken(string userName)
        {
            var userRepository = _userRepositoryFactory.GetUserRepository();
            return userRepository.CheckUserNameExists(userName);
        }
    }
}
