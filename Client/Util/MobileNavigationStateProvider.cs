using System;

namespace AuthServer.Client.Util
{
    public class MobileNavigationStateProvider
    {
        public event Action<ActiveContentEnum>? ActiveContentChanged;
        private ActiveContentEnum _activeContent = ActiveContentEnum.Sidebar;

        public ActiveContentEnum GetActiveContent()
        {
            return _activeContent;
        }

        public void ToggleActive()
        {
            switch (_activeContent)
            {
                case ActiveContentEnum.Content:
                    SetSidebarActive();
                    break;
                case ActiveContentEnum.Sidebar:
                    SetContentActive();
                    break;
            }
        }

        public void SetContentActive()
        {
            SetActiveContent(ActiveContentEnum.Content);
        }

        public void SetSidebarActive()
        {
            SetActiveContent(ActiveContentEnum.Sidebar);
        }

        private void SetActiveContent(ActiveContentEnum content)
        {
            _activeContent = content;
            ActiveContentChanged?.Invoke(content);
        }

        public enum ActiveContentEnum
        {
            Sidebar = 0,
            Content = 1
        }
    }
}
