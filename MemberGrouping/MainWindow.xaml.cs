using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MemberGrouping
{
    public partial class MainWindow : Window
    {
        private List<string> _files = new List<string>();

        public Collection<Point> selectedMembersXY = new Collection<Point>();

        public MainWindow()
        {
            InitializeComponent();
            PutMembersInGrid();
            CreatePhotos();
        }

        private void PutMembersInGrid()
        {
            var di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + "Members");
            var files = di.GetFiles("*.*");
            _files = files.Select(t => t.FullName).ToList();
            personCountInEachGroupSlider.Maximum = _files.Count;
        }

        private void CreatePhotos()
        {
            int index = 0;
            foreach (var file in _files)
            {
                var image = new Image()
                {
                    Source = new BitmapImage(new Uri(file)),
                    Clip = new EllipseGeometry() { RadiusX = 40, RadiusY = 40, Center = new Point(50, 50) }
                };
                image.MouseDown += Image_MouseDown;
                Grid1.Children.Add(image);
                Grid.SetRow(image, index / 4);
                Grid.SetColumn(image, index % 4);
                index++;
            }
        }

        private void Image_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var row = Grid.GetRow(sender as Image);
            var column = Grid.GetColumn(sender as Image);

            var di = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            var checkFile = di.GetFiles("check1.png").FirstOrDefault();
            var checkImage = new Image()
            {
                Source = new BitmapImage(new Uri(checkFile.FullName)),
                Clip = new EllipseGeometry() { RadiusX = 40, RadiusY = 40, Center = new Point(50, 50) },
                Opacity = 0.5
            };
            checkImage.MouseDown += CheckImage_MouseDown;
            Grid1.Children.Add(checkImage);
            Grid.SetRow(checkImage, row);
            Grid.SetColumn(checkImage, column);
            selectedMembersXY.Add(new Point(row, column));
            TbMemberCount.Text = selectedMembersXY.Count.ToString();
        }

        private void CheckImage_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var row = Grid.GetRow(sender as Image);
            var column = Grid.GetColumn(sender as Image);

            Grid1.Children.Remove(sender as Image);

            selectedMembersXY.Remove(new Point(row, column));

            TbMemberCount.Text = selectedMembersXY.Count.ToString();
        }

        private void ListAllMembersRamdomly()
        {
            Collection<Image> allMembers = new Collection<Image>();
            int index = 0;
            foreach (Image member in Grid1.Children)
            {
                if (selectedMembersXY.Any(p => (p.X * 4 + p.Y) == index))
                {
                    Image newMember = new Image() { Source = member.Source };
                    newMember.Tag = Guid.NewGuid().ToString();
                    newMember.Width = 70;
                    newMember.Height = 70;
                    newMember.Clip = new EllipseGeometry()
                    {
                        RadiusX = 30,
                        RadiusY = 30,
                        Center = new Point(35, 35)
                    };
                    allMembers.Add(newMember);
                }

                index++;
            }
            MembersBubbleSort(allMembers);

            PrintAllMembersInGroup(allMembers);
        }

        private void PrintAllMembersInGroup(Collection<Image> allMembers)
        {
            int memberCountInEachGroup = (int)personCountInEachGroupSlider.Value;
            int groupCount = selectedMembersXY.Count / memberCountInEachGroup
                + ((selectedMembersXY.Count % memberCountInEachGroup > 0) ? 1 : 0);
            TbTotalGroup.Text = groupCount.ToString();
            int memberIndex = 0;
            for (int groupIndex = 0; groupIndex < groupCount;)
            {
                WrapPanel wrapPanel = new WrapPanel()
                {
                    Margin = new Thickness(5) { },
                    Background = new SolidColorBrush() { Color = Color.FromRgb(240, 240, 240) }
                };
                Border border = new Border()
                {
                    Margin = new Thickness(4) { },
                    CornerRadius = new CornerRadius(15),
                    Background = new SolidColorBrush()
                    {
                        Color = Color.FromRgb(240, 240, 240)
                    }
                };
                border.Child = wrapPanel;
                wrapPanel.Children.Add(new Label() { Content = numToAlpha(groupIndex + 65), FontSize = 70, Foreground = new SolidColorBrush() { Color = Color.FromRgb(62, 86, 125) } });
                for (int i = 0; i < memberCountInEachGroup && memberIndex < selectedMembersXY.Count; i++)
                {
                    wrapPanel.Children.Add(allMembers[memberIndex]);
                    memberIndex++;
                }
                GroupedResultPanel.Children.Add(border);
                groupIndex++;
            }
        }

        public string numToAlpha(int num)
        {
            string strAlpha = "";
            strAlpha += ((char)num).ToString();
            return strAlpha;
        }

        public void BubbleSort(string[] id)
        {
            int i, j;
            string temp;
            bool exchange;
            for (i = 0; i < id.Length; i++)
            {
                exchange = false;
                for (j = id.Length - 2; j >= i; j--)
                {
                    if (string.Compare(id[j + 1], id[j]) < 0)
                    {
                        temp = id[j + 1];
                        id[j + 1] = id[j];
                        id[j] = temp;
                        exchange = true;
                    }
                }
                if (!exchange)
                {
                    break;
                }
            }
        }

        public void MembersBubbleSort(Collection<Image> members)
        {
            int i, j;
            Image temp;
            bool exchange;
            for (i = 0; i < selectedMembersXY.Count; i++)
            {
                exchange = false;
                for (j = selectedMembersXY.Count - 2; j >= i; j--)
                {
                    if (string.Compare(members[j + 1].Tag.ToString(), members[j].Tag.ToString()) < 0)
                    {
                        temp = members[j + 1];
                        members[j + 1] = members[j];
                        members[j] = temp;
                        exchange = true;
                    }
                }
                if (!exchange)
                {
                    break;
                }
            }
        }

        private void StartGroupBtn_Click(object sender, RoutedEventArgs e)
        {
            GroupedResultPanel.Children.Clear();
            ListAllMembersRamdomly();
        }

        private void personCountInEachGroupSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int memberCountInEachGroup = (int)personCountInEachGroupSlider.Value;
            int groupCount = selectedMembersXY.Count / memberCountInEachGroup
                + ((selectedMembersXY.Count % memberCountInEachGroup > 0) ? 1 : 0);
            if (TbTotalGroup != null)
            {
                TbTotalGroup.Text = groupCount.ToString();
            }
        }
    }
}
