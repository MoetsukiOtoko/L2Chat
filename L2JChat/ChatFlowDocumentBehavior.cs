using L2JChat.MVVM.Model;
using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace L2JChat
{
    sealed class ChatFlowDocumentBehavior : Behavior<FlowDocument>
    {
        // This is our dependency property for the messages
        public static readonly DependencyProperty ChatMessagesProperty =
          DependencyProperty.Register(
            nameof(ChatMessages),
            typeof(ObservableCollection<ChatMessage>),
            typeof(ChatFlowDocumentBehavior),
            new PropertyMetadata(defaultValue: null, ChatMessagesChanged));
        public ObservableCollection<ChatMessage> ChatMessages
        {
            get => (ObservableCollection<ChatMessage>)GetValue(ChatMessagesProperty);
            set => SetValue(ChatMessagesProperty, value);
        }

        // This defines how our items will look like
        public DataTemplate ItemTemplate { get; set; }

        // This method will be called by the framework when the behavior attaches to flow document
        protected override void OnAttached()
        {
            RefreshMessages();
        }

        private static void ChatMessagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ChatFlowDocumentBehavior b))
            {
                return;
            }

            if (e.OldValue is ObservableCollection<ChatMessage> oldValue)
            {
                oldValue.CollectionChanged -= b.MessagesCollectionChanged;
            }

            if (e.NewValue is ObservableCollection<ChatMessage> newValue)
            {
                newValue.CollectionChanged += b.MessagesCollectionChanged;
            }

            // When the binding engine updates the dependency property value,
            // update the flow doocument
            b.RefreshMessages();
        }

        private void MessagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddNewItems(e.NewItems.OfType<ChatMessage>());
                    break;

                case NotifyCollectionChangedAction.Reset:
                    AssociatedObject.Blocks.Clear();
                    break;
            }
        }

        private void RefreshMessages()
        {
            if (AssociatedObject == null)
            {
                return;
            }

            AssociatedObject.Blocks.Clear();
            if (ChatMessages == null)
            {
                return;
            }

            AddNewItems(ChatMessages);
        }

        private void AddNewItems(IEnumerable<ChatMessage> items)
        {
            foreach (var message in items)
            {
                // If the template was provided, create an instance from the template;
                // otherwise, create a default non-styled paragraph instance
                var newItem = (Paragraph)(ItemTemplate?.LoadContent() as Fragment)?.Content ?? new Paragraph();

                if(message.Type == "Message")
                {
                    var bc = new BrushConverter();
                    newItem.Inlines.Add(new Bold(new Run(message.Sender + ": "))
                    {
                        Foreground = (Brush)bc.ConvertFrom(message.Color)
                    });
                    newItem.Inlines.Add(new Run(message.Message) 
                    {
                        Foreground = (Brush)bc.ConvertFrom("#FFFFFC")
                    });
                    AssociatedObject.Blocks.Add(newItem);
                }
                if(message.Type == "Connected")
                {
                    newItem.Inlines.Add(new Run(message.Message)
                    {
                        Foreground = Brushes.Yellow
                    });
                    AssociatedObject.Blocks.Add(newItem);
                }
                if (message.Type == "Disconnected")
                {
                    newItem.Inlines.Add(new Run(message.Message)
                    {
                        Foreground = Brushes.Red
                    });
                    AssociatedObject.Blocks.Add(newItem);
                }
            }
        }
    }
}