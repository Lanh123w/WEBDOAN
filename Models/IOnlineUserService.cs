namespace WEBDOAN.Models;


    public interface IOnlineUserService
    {
        void UserActive(string userId);
        int GetOnlineCount();
    }

