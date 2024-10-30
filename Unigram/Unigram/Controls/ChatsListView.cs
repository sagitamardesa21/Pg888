using Unigram.Controls.Cells;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;

namespace Unigram.Controls
{
    public class ChatsListViewItem : ListViewItem
    {
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            return new ChatsListViewItemAutomationPeer(this);
        }
    }

    public class ChatsListViewItemAutomationPeer : ListViewItemAutomationPeer
    {
        private ChatsListViewItem _owner;

        public ChatsListViewItemAutomationPeer(ChatsListViewItem owner)
            : base(owner)
        {
            _owner = owner;
        }

        protected override string GetNameCore()
        {
            if (_owner.ContentTemplateRoot is ChatCell cell)
            {
                return cell.GetAutomationName() ?? base.GetNameCore();
            }

            return base.GetNameCore();
        }
    }
}
