namespace WEBDOAN.Models;


    public interface IOnlineUserService
    {
        void UserActive(string userId);
        void UserInactive(string userId);
    int GetOnlineCount();
    }

