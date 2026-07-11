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

    extension(Pickup pickup)
    {
        public string Name
        {
            get
            {
                try
                {
                    return pickup.Type.GetName() ?? pickup.Type.ToString();
                }
                catch (NullReferenceException)
                {
                    return pickup.Type.ToString();
                }
            }
        }
    }
}