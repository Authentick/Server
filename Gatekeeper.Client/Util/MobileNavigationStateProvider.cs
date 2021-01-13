using System;

namespace Gatekeeper.Client.Util
{
    public class MobileNavigationStateProvider
    {
        public event Action<ActiveContentEnum>? ActiveContentChanged;
        private ActiveContentEnum _activeContent = ActiveContentEnum.Sidebar;
        public string? Name;

        public ActiveContentEnum GetActiveContent()
        {
            return _activeContent;
        }

        public void SetContentActive(string name)
        {
            Name = name;
            SetActiveContent(ActiveContentEnum.Content);
        }

        public void SetSidebarActive(string name)
        {
            Name = name;
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
