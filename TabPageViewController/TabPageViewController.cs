using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace TabPageViewController
{
    public class TabItem
    {
        public string title { get; set; }
        public UIViewController viewController { get; set; }
    }
    public class TabPageViewController : UIPageViewController,
        IUIPageViewControllerDataSource,
        IUIPageViewControllerDelegate,
        IUIScrollViewDelegate
    {
        public bool isInfinity { get; set; } = false;
        public TabPageOption option { get; set; } = new TabPageOption();
        public TabItem[] tabItems { get; set; }

        public int? currentIndex
        {
            get
            {
                if (ViewControllers == null) return null;
                var viewController = ViewControllers.First();
                return 0;// tabItems.Index(x => x.viewController == viewController);
            }
        }

        public int beforeIndex { get; set; } = 0;
        public int tabItemCount { get { return tabItems.Length; } }
        public nfloat defaultContentOffsetX { get { return this.View.Bounds.Width; } }
        public bool shouldScrollCurrentBar { get; set; }
        public TabView tabView { get; set; } //= configuredTabView();
        public UIView statusView { get; set; }
        public NSLayoutConstraint statusViewHeightConstraint { get; set; }
        public NSLayoutConstraint tabBarTopConstraint { get; set; }

        public TabPageViewController() : base(UIPageViewControllerTransitionStyle.Scroll, UIPageViewControllerNavigationOrientation.Horizontal, null) { }

        public TabPageViewController(NSCoder coder) : base(UIPageViewControllerTransitionStyle.Scroll, UIPageViewControllerNavigationOrientation.Horizontal, null) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            setupPageViewController();
            setupScrollView();
            updateNavigationBar();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            if (tabView.Superview == null)
            {
                //tabView = configuredTabView();
            }

            if (currentIndex.HasValue && isInfinity)
            {
                //tabView.updateCurrentIndex(currentIndex, shouldScroll: true);
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            updateNavigationBar();
            //tabView.layouted = true;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            NavigationController.NavigationBar.ShadowImage = null;
            NavigationController?.NavigationBar.SetBackgroundImage(null, UIBarMetrics.Default);
        }

        public void displayControllerWithIndex(int index, UIPageViewControllerNavigationDirection direction, bool animated)
        {
            beforeIndex = index;
            shouldScrollCurrentBar = false;
            UIViewController nextViewControllers = tabItems[index].viewController;

            /*
             let completion: ((Bool) -> Void) = { [weak self] _ in
            self?.shouldScrollCurrentBar = true
            self?.beforeIndex = index
        }

             */

            SetViewControllers(new[] { nextViewControllers }, direction, animated, null);

            if (IsViewLoaded) return;
            //tabView.updateCurrentIndex(index, true);
        }

        private void setupPageViewController()
        {
            DataSource = this;
            Delegate = this;

            AutomaticallyAdjustsScrollViewInsets = false;

            SetViewControllers(new[] { tabItems[beforeIndex].viewController }, UIPageViewControllerNavigationDirection.Forward, false, null);
        }

        private void setupScrollView()
        {
            var scrollView = View.Subviews.First(x => x.Subviews is UIScrollView) as UIScrollView;
            if (scrollView != null)
            {
                scrollView.ScrollsToTop = false;
                scrollView.Delegate = this;
                scrollView.BackgroundColor = option.pageBackgroundColor;
            }
        }

        private void updateNavigationBar()
        {
            var navigationBar = NavigationController?.NavigationBar;
            if(navigationBar != null)
            {
                navigationBar.ShadowImage = new UIImage();
                navigationBar.SetBackgroundImage(option.tabBackgroundImage, UIBarMetrics.Default);
                navigationBar.Translucent = option.isTranslucent;
            }
        }

        private TabView configuredTabView()
        {
            var _tabView = new TabView(isInfinity, option);
            tabView.TranslatesAutoresizingMaskIntoConstraints = false;

            //add constraints
            var height = NSLayoutConstraint.Create(tabView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.Height, 1.0f, option.tabHeight);
            tabView.AddConstraint(height);
            View.AddSubview(tabView);

            var top = NSLayoutConstraint.Create(tabView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, TopLayoutGuide, NSLayoutAttribute.Bottom, 1.0f, 0.0f);
            var left = NSLayoutConstraint.Create(tabView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View, NSLayoutAttribute.Leading, 1.0f, 0.0f);
            var right = NSLayoutConstraint.Create(tabView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, tabView, NSLayoutAttribute.Trailing, 1.0f, 0.0f);

            View.AddConstraints(new[] { top, left, right });

            tabView.pageTabItems = tabItems.Select(x => x.title).ToArray();
            //tabView.updateCurrentIndex(beforeIndex, true);

            //TODO add remaining here

            return tabView;
        }

        private void setupStatusView()
        {
            var statusView = new UIView();
            statusView.BackgroundColor = option.tabBackgroundColor;
            statusView.TranslatesAutoresizingMaskIntoConstraints = false;
            View.AddSubview(statusView);

            var top = NSLayoutConstraint.Create(statusView, NSLayoutAttribute.Top, NSLayoutRelation.Equal, View, NSLayoutAttribute.Top, 1.0f, 0.0f);
            var left = NSLayoutConstraint.Create(statusView, NSLayoutAttribute.Leading, NSLayoutRelation.Equal, View, NSLayoutAttribute.Leading, 1.0f, 0.0f);
            var right = NSLayoutConstraint.Create(statusView, NSLayoutAttribute.Trailing, NSLayoutRelation.Equal, statusView, NSLayoutAttribute.Trailing, 1.0f, 0.0f); 
            var height = NSLayoutConstraint.Create(statusView, NSLayoutAttribute.Height, NSLayoutRelation.Equal, null, NSLayoutAttribute.Height, 1.0f, TopLayoutGuide.Length);

            View.AddConstraints(new[] { top, left, right, height });

            statusViewHeightConstraint = height;
            this.statusView = statusView;
        }

        public void updateNavigationBarHidden(bool hidden, bool animated)
        {
            if (NavigationController == null) return;

            switch (option.hidesTopViewOnSwipeType)
            {
                case HidesTopContentsOnSwipeType.none:
                    break;
                case HidesTopContentsOnSwipeType.tabbar:
                    updateTabBarOrigin(hidden);
                    break;
                case HidesTopContentsOnSwipeType.navigationBar:
                    if (hidden)
                    {
                        NavigationController.SetNavigationBarHidden(true, true);
                    }
                    else
                    {
                        showNavigationBar();
                    }
                    break;
                case HidesTopContentsOnSwipeType.all:
                    updateTabBarOrigin(hidden);
                    if (hidden)
                    {
                        NavigationController.SetNavigationBarHidden(true, true);
                    }
                    else
                    {
                        showNavigationBar();
                    }
                    break;
                default:
                    break;
            }

            if(statusView == null)
            {
                setupStatusView();
            }

            if (statusViewHeightConstraint != null)
                statusViewHeightConstraint.Constant = TopLayoutGuide.Length;
        }

        public void showNavigationBar()
        {
            if (NavigationController == null) return;
            if (NavigationController.NavigationBarHidden) return;
            if (tabBarTopConstraint == null) return;

            if(option.hidesTopViewOnSwipeType != HidesTopContentsOnSwipeType.none)
            {
                tabBarTopConstraint.Constant = 0.0f;
                UIView.Animate(UINavigationController.HideShowBarDuration, () => { View.LayoutIfNeeded(); });
            }

            NavigationController.SetNavigationBarHidden(false, true);
        }

        private void updateTabBarOrigin(bool hidden)
        {
            if (tabBarTopConstraint == null) return;

            tabBarTopConstraint.Constant = hidden ? -(20.0f + option.tabHeight) : 0.0f;
            UIView.Animate(UINavigationController.HideShowBarDuration, () => { View.LayoutIfNeeded(); });
        }


        #region IUIPageViewControllerDataSource
        private UIViewController nextViewController(UIViewController viewController, bool isAfter)
        {
            var index = Array.FindIndex(tabItems, x => x.viewController == viewController);
            if (index < 0) return null;
            if (isAfter) index += 1;
            else index -= 1;

            if(isInfinity)
            {
                if(index < 0)
                {
                    index = tabItems.Length - 1;
                }
                else if (index == tabItems.Length)
                {
                    index = 0;
                }
            }

            if(index >= 0 && index < tabItems.Length)
            {
                return tabItems[index].viewController;
            }
            return null;
        }
        UIViewController IUIPageViewControllerDataSource.GetPreviousViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
        {
            return nextViewController(referenceViewController, isAfter: false);
        }

        UIViewController IUIPageViewControllerDataSource.GetNextViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
        {
            return nextViewController(referenceViewController, isAfter: true);
        }
        #endregion

        #region IUIPageViewControllerDelegate


        [Export("pageViewController:didFinishAnimating:previousViewControllers:transitionCompleted:")]
        public void DidFinishAnimating(UIPageViewController pageViewController, bool finished, UIViewController[] previousViewControllers, bool completed)
        {
            if(currentIndex.HasValue && currentIndex < tabItemCount)
            {
                //tabView.updateCurrentIndex(currentIndex, shouldScroll: false);
                beforeIndex = currentIndex.Value;
            }

            //tabView.updateCollectionViewUserInteractionEnabled(true);
        }

        public void WillTransition(UIPageViewController pageViewController, UIViewController[] pendingViewControllers)
        {
            shouldScrollCurrentBar = true;
            //tabView.ScrollToHorizontalCenter();

            // Order to prevent the the hit repeatedly during animation
            //tabView.UpdateCollectionViewUserInteractionEnabled(false);
        }

        #endregion

        #region IUIScrollViewDelegate

        public void Scrolled(UIScrollView scrollView)
        {
            if(scrollView.ContentOffset.X == defaultContentOffsetX || !shouldScrollCurrentBar)
            {
                return;
            }

            int index;

            if(scrollView.ContentOffset.X > defaultContentOffsetX)
            {
                index = beforeIndex + 1;
            }
            else
            {
                index = beforeIndex - 1;
            }

            if(index == tabItemCount)
            {
                index = 0;
            }
            else if(index < 0)
            {
                index = tabItemCount - 1;
            }

            var scrollOffsetX = scrollView.ContentOffset.X - View.Frame.Width;
            //tabView.scrollCurrentBarView(index, scrollOffsetX);
        }
        public void DecelerationEnded(UIScrollView scroolView)
        {
            //tabView.updateCurrentIndex(beforeIndex, true);
        }
        #endregion
    }
}