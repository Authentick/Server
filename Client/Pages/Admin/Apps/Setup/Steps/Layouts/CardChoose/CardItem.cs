namespace AuthServer.Client.Pages.Admin.Apps.Setup.Steps.Layouts.CardChoose
{
    public class CardItem
    {
        public readonly string Name;
        public readonly string Description;
        public readonly string Icon;

        public CardItem(
            string name, 
            string description, 
            string icon)
        {
            Name = name;
            Description = description;
            Icon = icon;
        }
    }
}
