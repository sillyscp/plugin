using InventorySystem.Items;
using LabApi.Features.Wrappers;

namespace LogAssistant.Extensions;

public static class ItemExtensions
{
    extension(Item item)
    {
        public string Name
        {
            get
            {
                try
                {
                    return item.Base switch
                    {
                        IItemNametag name => name.Name,
                        IItemNickname nickname => nickname.Nickname,
                        _ => item.Type.GetName() ?? item.Type.ToString()
                    };
                }
                catch (NullReferenceException)
                {
                    return item.Type.ToString();
                }
            }
        }
    }
}