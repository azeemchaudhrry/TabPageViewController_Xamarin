// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using Foundation;
using System;
using System.CodeDom.Compiler;

namespace TabPageViewController_Xamarin
{
    [Register ("TabCollectionCell")]
    partial class TabCollectionCell
    {
        [Outlet]
        UIKit.UIView currentBarView { get; set; }


        [Outlet]
        UIKit.NSLayoutConstraint currentBarViewHeightConstraint { get; set; }


        [Outlet]
        UIKit.UILabel itemLabel { get; set; }


        [Action ("tabItemTouchUpInside:")]
        partial void tabItemTouchUpInside (Foundation.NSObject sender);

        void ReleaseDesignerOutlets ()
        {
        }
    }
}