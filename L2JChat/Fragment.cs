using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows;

namespace L2JChat
{
    [ContentProperty("Content")]
    sealed class Fragment : FrameworkElement
    {
        public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
          nameof(Content),
          typeof(FrameworkContentElement),
          typeof(Fragment));

        public FrameworkContentElement Content
        {
            get => (FrameworkContentElement)GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }
    }
}
