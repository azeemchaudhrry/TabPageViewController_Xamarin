using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace TabPageViewController_Xamarin
{
    public partial class TabCollectionCell : UICollectionViewCell
    {
        public static readonly NSString Key = new NSString("TabCollectionCell");
        public static readonly UINib Nib;
        
        public Action<TabView, TabCollectionCell> tabItemButtonPressedBlock;

        private TabPageOption _options;
        public TabPageOption option
        {
            get { return _options; }
            set
            {
                _options = value;
                currentBarViewHeightConstraint.Constant = _options.currentBarHeight;
            }
        }

        private string _item = "";
        public string item
        {
            get { return _item; }
            set
            {
                _item = value;
                itemLabel.Text = _item;
                itemLabel.InvalidateIntrinsicContentSize();
                InvalidateIntrinsicContentSize();
            }
        }

        private bool _isCurrent = false;
        public bool isCurrent 
        {
            get { return _isCurrent; }
            set
            {
                _isCurrent = value;
                currentBarView.Hidden = !_isCurrent;
                if (_isCurrent)
                {
                    highlightTitle();
                }
                else
                {
                    unHighlightTitle();
                }
                currentBarView.BackgroundColor = option.currentColor;
                LayoutIfNeeded();
            }
        }

        static TabCollectionCell()
        {
            Nib = UINib.FromName("TabCollectionCell", NSBundle.MainBundle);
        }

        protected TabCollectionCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            currentBarView.Hidden = true;
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            if (item.Length == 0) return CGSize.Empty;
            return IntrinsicContentSize;
        }

        public override CGSize IntrinsicContentSize
        {
            get 
            {
                nfloat width = 0.0f;
                if(option.tabWidth.HasValue && option.tabWidth > 0.0)
                {
                    width = option.tabWidth.Value;
                }
                else
                {
                    width = itemLabel.IntrinsicContentSize.Width + option.tabMargin * 2;
                }

                var size = new CGSize(width, option.tabHeight);
                return size; 
            }
        }

        public void hideCurrentBarView()
        {
            currentBarView.Hidden = true;
        }

        public void showCurrentBarView()
        {
            currentBarView.Hidden = false;
        }

        public void highlightTitle()
        {
            itemLabel.TextColor = option.currentColor;
            itemLabel.Font = UIFont.BoldSystemFontOfSize(option.fontSize);
        }

        public void unHighlightTitle()
        {
            itemLabel.TextColor = option.defaultColor;
            itemLabel.Font = UIFont.SystemFontOfSize(option.fontSize);
        }

        public static string cellIdentifier()
        {
            return "TabCollectionCell";
        }

        partial void tabItemTouchUpInside(Foundation.NSObject sender)
        {
            //tabItemButtonPressedBlock();
        }
    }
}