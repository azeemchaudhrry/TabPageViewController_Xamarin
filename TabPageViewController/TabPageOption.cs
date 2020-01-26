using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using UIKit;

namespace TabPageViewController_Xamarin
{
    public class TabPageOption
    {
        public nfloat fontSize { get; set; } = UIFont.SystemFontSize;
        public UIColor currentColor { get; set; } = UIColor.FromRGBA(red: 105 / 255, green: 182 / 255, blue: 245 / 255, alpha: 1.0f);
        public UIColor defaultColor { get; set; } = UIColor.FromRGBA(red: 153 / 255, green: 153 / 255, blue: 153 / 255, alpha: 1.0f);
        public nfloat tabHeight { get; set; } = 32.0f;
        public nfloat tabMargin { get; set; } = 20.0f;
        public nfloat? tabWidth { get; set; }
        public nfloat currentBarHeight { get; set; } = 2.0f;
        public UIColor tabBackgroundColor { get; set; } = UIColor.White;
        public UIColor pageBackgroundColor { get; set; } = UIColor.White;
        public bool isTranslucent { get; set; } = true;
        public HidesTopContentsOnSwipeType hidesTopViewOnSwipeType { get; set; } = HidesTopContentsOnSwipeType.none;

        internal nfloat tabBarAlpha
        {
            get
            {
                return isTranslucent ? 0.95f : 1.0f;
            }
        }

        internal UIImage tabBackgroundImage
        {
            get
            {
                return convertImage();
            }
        }

        public TabPageOption()
        {

        }

        private UIImage convertImage()
        {
            var rect = new CGRect(x: 0, y: 0, width: 1, height: 1);
            UIGraphics.BeginImageContext(rect.Size);
            var context = UIGraphics.GetCurrentContext();
            var backgroundColor = tabBackgroundColor.ColorWithAlpha(tabBarAlpha).CGColor;
            context?.SetFillColor(backgroundColor);
            context?.FillRect(rect);
            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
        }
    }
}