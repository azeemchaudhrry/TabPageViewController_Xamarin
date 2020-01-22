using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace TabPageViewController
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
                //
            }
        }

        private string _item = "";
        public string item
        {
            get { return _item; }
            set
            {
                //
            }
        }

        private bool _isCurrent = false;
        public bool isCurrent 
        {
            get { return _isCurrent; }
            set
            {
                //currentBarView.isHidden = !_isCurrent;
                if (_isCurrent)
                {
                    highlightTitle();
                }
                else
                {
                    unHighlightTitle();
                }
                //currentBarView.BackgroundColor = option.currentColor;
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

            //currentBarView.IsHidden = true;
        }

        public override CGSize SizeThatFits(CGSize size)
        {
            if (item.Length == 0) return CGSize.Empty;
            //return intrinsicContentSize;
            return base.SizeThatFits(size);
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
                    //width = itemLabel.intrinsicContentSize.width + option.tabMargin * 2;
                }

                var size = new CGSize(width, option.tabHeight);
                return size; 
            }
        }

        private void hideCurrentBarView()
        {
            //hideCurrentBarView.isHidden = true;
        }

        private void showCurrentBarView()
        {
            //hideCurrentBarView.isHidden = false;
        }

        private void highlightTitle()
        {
            //itemLabel.textColor = option.currentColor
            //itemLabel.font = UIFont.boldSystemFont(ofSize: option.fontSize)
        }

        private void unHighlightTitle()
        {
            //itemLabel.textColor = option.defaultColor
            //itemLabel.font = UIFont.systemFont(ofSize: option.fontSize)
        }

        //
    }
}