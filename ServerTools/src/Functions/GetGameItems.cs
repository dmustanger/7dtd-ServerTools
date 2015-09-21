using System.Collections.Generic;

namespace ServerTools
{
    public class GameItems
    {
        public static SortedDictionary<string, ItemValue> _gameItems = new SortedDictionary<string, ItemValue>();

        public static void LoadGameItems()
        {
            NGuiInvGridCreativeMenu _menu = new NGuiInvGridCreativeMenu();
            foreach (ItemStack _itemStack in _menu.GetAllItems())
            {
                ItemClass _itemClass = ItemClass.list[_itemStack.itemValue.type];
                string name = _itemClass.GetItemName();
                if (name != null && name.Length > 0 && !_gameItems.ContainsKey(name))
                {
                    _gameItems.Add(name, _itemStack.itemValue);
                }
            }
            foreach (ItemStack _itemStack in _menu.GetAllBlocks())
            {
                ItemClass _itemClass = ItemClass.list[_itemStack.itemValue.type];
                string name = _itemClass.GetItemName();
                if (name != null && name.Length > 0 && !_gameItems.ContainsKey(name))
                {
                    _gameItems.Add(name, _itemStack.itemValue);
                }
            }
        }
    }
}