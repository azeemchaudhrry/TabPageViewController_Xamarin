using System;
using TabPageViewController_Xamarin;
using UIKit;

namespace TabbedExample
{
    public partial class FirstViewController : TabPageViewController
    {
        public FirstViewController(IntPtr handle) : base(handle)
        {
            setupTabLayout();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        private void setupTabLayout()
        {
            var vc1 = new UIViewController();
            vc1.View.BackgroundColor = UIColor.White;// new UIColor(red: 251 / 255, green: 252 / 255, blue: 149 / 255, alpha: 1.0f);
            var vc2 = new UIViewController();
            vc2.View.BackgroundColor = new UIColor(red: 252 / 255, green: 150 / 255, blue: 149 / 255, alpha: 1.0f);
            var vc3 = new UIViewController();
            vc3.View.BackgroundColor = new UIColor(red: 149 / 255, green: 218 / 255, blue: 252 / 255, alpha: 1.0f);
            var vc4 = new UIViewController();
            vc4.View.BackgroundColor = new UIColor(red: 149 / 255, green: 252 / 255, blue: 197 / 255, alpha: 1.0f);
            var vc5 = new UIViewController();
            vc5.View.BackgroundColor = new UIColor(red: 252 / 255, green: 182 / 255, blue: 106 / 255, alpha: 1.0f);
            tabItems = new TabItem[] { new TabItem(vc1, "Mon."), new TabItem(vc2, "Tue."), new TabItem(vc3, "Wed."), new TabItem(vc4, "Thu."), new TabItem(vc5, "Fri.") };
            isInfinity = true;
            option.currentColor = UIColor.White;// new UIColor(red: 246 / 255, green: 175 / 255, blue: 32 / 255, alpha: 1.0f);
            option.tabMargin = 30.0f;
        }
    }
}