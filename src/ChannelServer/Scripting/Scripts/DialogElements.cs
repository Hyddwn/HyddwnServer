// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting.Scripts
{
    public class DialogElement
    {
        public DialogElement()
        {
            Children = new List<DialogElement>();
        }

        public DialogElement(params DialogElement[] elements)
            : this()
        {
            Children.AddRange(elements);
        }

        public List<DialogElement> Children { get; protected set; }

        public static implicit operator DialogElement(string msg)
        {
            return (DialogText) msg;
        }

        public DialogElement Add(params DialogElement[] elements)
        {
            Children.AddRange(elements);
            return this;
        }

        public DialogElement Insert(int index, params DialogElement[] elements)
        {
            index = Math2.Clamp(0, Children.Count, index);

            Children.InsertRange(index, elements);

            return this;
        }

        public DialogElement Replace(int index, params DialogElement[] elements)
        {
            index = Math2.Clamp(0, Children.Count, index);

            if (Children.Count != 0)
                Children.RemoveAt(index);
            Children.InsertRange(index, elements);

            return this;
        }

        public virtual void Render(ref StringBuilder sb)
        {
            foreach (var child in Children)
                child.Render(ref sb);
        }

        /// <summary>
        ///     Renders this and its child elements.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            Render(ref sb);
            return sb.ToString();
        }
    }

    /// <summary>
    ///     Simple text. Strings passed to Msg are converted into this.
    /// </summary>
    public class DialogText : DialogElement
    {
        public DialogText(string format, params object[] args)
        {
            Text = string.Format(format, args);
        }

        public string Text { get; set; }

        /// <summary>
        ///     Performs an implicit conversion from <see cref="System.String" /> to <see cref="DialogText" />.
        /// </summary>
        /// <param name="msg">The msg.</param>
        /// <returns>
        ///     A new DialogText instance with the string as the text.
        /// </returns>
        public static implicit operator DialogText(string msg)
        {
            return new DialogText(msg);
        }

        public override void Render(ref StringBuilder sb)
        {
            sb.Append(Text);

            base.Render(ref sb);
        }
    }

    /// <summary>
    ///     Changes the NPC portrait, displayed at the upper left of the dialog.
    /// </summary>
    public class DialogPortrait : DialogElement
    {
        public DialogPortrait(string text)
        {
            if (text == null)
                Text = "NONE";
            else
                Text = text;
        }

        public string Text { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<npcportrait name='{0}' />", Text);

            base.Render(ref sb);
        }
    }

    /// <summary>
    ///     Changes the name of the speaking person (at the top of the dialog).
    /// </summary>
    public class DialogTitle : DialogElement
    {
        public DialogTitle(string text)
        {
            if (text == null)
                Text = "NONE";
            else
                Text = text;
        }

        public string Text { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<title name='{0}' />", Text);

            base.Render(ref sb);
        }
    }

    /// <summary>
    ///     Shows the configured hotkey for the given id.
    /// </summary>
    public class DialogHotkey : DialogElement
    {
        public DialogHotkey(string text)
        {
            Text = text;
        }

        public string Text { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<hotkey name='{0}' />", Text);

            base.Render(ref sb);
        }
    }

    /// <summary>
    ///     A button is displayed at the bottom of the dialog, and can be clicked.
    ///     The keyword of the button is sent to the server and can be read using Select.
    /// </summary>
    public class DialogButton : DialogElement
    {
        public DialogButton(string text, string keyword = null, string onFrame = null)
        {
            Text = text;
            OnFrame = onFrame;

            if (keyword != null)
            {
                Keyword = keyword;
            }
            else
            {
                // Take text, prepend @, replace all non-numerics with _ and
                // make the string lower case, if no keyword was given.
                // Yea... hey, this is 10 times faster than Regex + ToLower!
                var sb = new StringBuilder();
                sb.Append('@');
                foreach (var c in text)
                    if (c >= '0' && c <= '9' || c >= 'a' && c <= 'z')
                        sb.Append(c);
                    else if (c >= 'A' && c <= 'Z')
                        sb.Append((char) (c + 32));
                    else
                        sb.Append('_');
                Keyword = sb.ToString();
            }
        }

        public string Text { get; set; }
        public string Keyword { get; set; }
        public string OnFrame { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<button title='{0}' keyword='{1}'", Text, Keyword);
            if (OnFrame != null)
                sb.AppendFormat(" onframe='{0}'", OnFrame);
            sb.Append(" />");
        }
    }

    /// <summary>
    ///     Changes the background music, for the duration of the dialog.
    /// </summary>
    public class DialogBgm : DialogElement
    {
        public DialogBgm(string file)
        {
            File = file;
        }

        public string File { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<music name='{0}'/>", File);
        }
    }

    /// <summary>
    ///     Shows an image in the center of the screen.
    /// </summary>
    public class DialogImage : DialogElement
    {
        public DialogImage(string name, bool localize = false, int width = 0, int height = 0)
        {
            File = name;
            Localize = localize;
            Width = width;
            Height = height;
        }

        public string File { get; set; }
        public bool Localize { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<image name='{0}'", File);
            if (Localize)
                sb.Append(" local='true'");
            if (Width != 0)
                sb.AppendFormat(" width='{0}'", Width);
            if (Height != 0)
                sb.AppendFormat(" height='{0}'", Height);

            sb.Append(" />");
        }
    }

    /// <summary>
    ///     Displays a list of options (buttons) in a separate window.
    ///     Result is sent like a regular button click.
    /// </summary>
    public class DialogList : DialogElement
    {
        public DialogList(string text, int height, string cancelKeyword, params DialogButton[] elements)
        {
            Height = height;
            Text = text;
            CancelKeyword = cancelKeyword;
            Add(elements);
        }

        public DialogList(string text, params DialogButton[] elements)
            : this(text, elements.Length, elements)
        {
        }

        public DialogList(string text, int height, params DialogButton[] elements)
            : this(text, height, "@end", elements)
        {
        }

        public string Text { get; set; }
        public string CancelKeyword { get; set; }
        public int Height { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<listbox page_size='{0}' title='{1}' cancel='{2}'>", Height, Text, CancelKeyword);
            base.Render(ref sb);
            sb.Append("</listbox>");
        }
    }

    /// <summary>
    ///     Shows a single lined input box. The result is sent as a regular
    ///     Select result.
    /// </summary>
    public class DialogInput : DialogElement
    {
        public DialogInput(string title = "Input", string text = "", byte maxLength = 20, bool cancelable = true)
        {
            Title = title;
            Text = text;
            MaxLength = maxLength;
            Cancelable = cancelable;
        }

        public string Title { get; set; }
        public string Text { get; set; }
        public byte MaxLength { get; set; }
        public bool Cancelable { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<inputbox title='{0}' caption='{1}' max_len='{2}' allow_cancel='{3}' />", Title, Text,
                MaxLength, Cancelable.ToString().ToLower());

            base.Render(ref sb);
        }
    }

    /// <summary>
    ///     Dialog automatically continues after x ms.
    /// </summary>
    public class DialogAutoContinue : DialogElement
    {
        public DialogAutoContinue(int duration)
        {
            Duration = duration;
        }

        public int Duration { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<autopass duration='{0}'/>", Duration);
        }
    }

    /// <summary>
    ///     Changes the facial expression of the portrait.
    ///     (Defined client sided in the db/npc/npcportrait_ani_* files.)
    /// </summary>
    public class DialogFaceExpression : DialogElement
    {
        public DialogFaceExpression(string expression)
        {
            Expression = expression;
        }

        public string Expression { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<face name='{0}'/>", Expression);
        }
    }

    /// <summary>
    ///     Plays a movie in a box in the center of the screen.
    ///     Files are taken from movie/.
    /// </summary>
    public class DialogMovie : DialogElement
    {
        public DialogMovie(string file, int width, int height, bool loop = true)
        {
            File = file;
            Width = width;
            Height = height;
            Loop = loop;
        }

        public string File { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Loop { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<movie name='{0}' width='{1}' height='{2}' loop='{3}' />", File, Width, Height,
                Loop.ToString().ToLower());
        }
    }

    /// <summary>
    ///     Opens minimap, which is usually hidden during conversations.
    /// </summary>
    public class DialogMinimap : DialogElement
    {
        public DialogMinimap(bool zoom, bool maxSize, bool center)
        {
            Zoom = zoom;
            MaxSize = maxSize;
            Center = center;
        }

        public bool Zoom { get; set; }
        public bool MaxSize { get; set; }
        public bool Center { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<openminimap zoom='{0}' max_size='{1}' center='{2}' />", Zoom.ToString().ToLower(),
                MaxSize.ToString().ToLower(), Center.ToString().ToLower());
        }
    }

    /// <summary>
    ///     Displays marker on minimap for specified duration.
    /// </summary>
    public class DialogShowPosition : DialogElement
    {
        public DialogShowPosition(int region, int x, int y, int remainingTime)
        {
            Region = region;
            X = x;
            Y = y;
            RemainingTime = remainingTime;
        }

        public int Region { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int RemainingTime { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<show_position region='{0}' pos='{1} {2}' remainingtime='{3}' />", Region, X, Y,
                RemainingTime);
        }
    }

    /// <summary>
    ///     Turns camera into the direction of the position.
    /// </summary>
    public class DialogShowDirection : DialogElement
    {
        public DialogShowDirection(int x, int y, int angle)
        {
            X = x;
            Y = y;
            Angle = angle;
        }

        public int X { get; set; }
        public int Y { get; set; }
        public int Angle { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<show_dir pos='{0} {1}' pitch='{2}' />", X, Y, Angle);
        }
    }

    /// <summary>
    ///     Changes the name displayed for the NPC for the rest of the conversation.
    /// </summary>
    public class DialogSetDefaultName : DialogElement
    {
        public DialogSetDefaultName(string name)
        {
            Name = name;
        }

        public string Name { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<defaultname name='{0}' />", Name);
        }
    }

    /// <summary>
    ///     Changes the name displayed for the NPC for the rest of the conversation.
    /// </summary>
    public class DialogSelectItem : DialogElement
    {
        public DialogSelectItem(string title, string caption, string tags)
        {
            Title = title;
            Caption = caption;
            Tags = tags;
        }

        public string Title { get; set; }
        public string Caption { get; set; }
        public string Tags { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<selectitem title='{0}' caption='{1}' stringid='{2}' />", Title, Caption, Tags);
        }
    }

    /// <summary>
    ///     Displays PTJ description window.
    /// </summary>
    public class DialogPtjDesc : DialogElement
    {
        public DialogPtjDesc(int questId, string name, string title, int maxAvailableJobs, int remainingJobs,
            int history)
        {
            QuestId = questId;
            Name = name;
            Title = title;
            MaxAvailableJobs = maxAvailableJobs;
            RemainingJobs = remainingJobs;
            History = history;
        }

        public int QuestId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int MaxAvailableJobs { get; set; }
        public int RemainingJobs { get; set; }
        public int History { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            var quest = ChannelServer.Instance.ScriptManager.QuestScripts.Get(QuestId);
            if (quest == null)
                throw new ArgumentException("DialogPtjDesc: Unknown quest '" + QuestId + "'.");

            var objective = quest.Objectives.First().Value;

            var now = ErinnTime.Now;
            var remainingHours = Math.Max(0, quest.DeadlineHour - now.Hour);
            var remainingJobs = Math2.Clamp(0, MaxAvailableJobs, RemainingJobs);

            sb.Append("<arbeit>");
            sb.AppendFormat("<name>{0}</name>", Name);
            sb.AppendFormat("<id>{0}</id>", QuestId);
            sb.AppendFormat("<title>{0}</title>", Title);
            foreach (var group in quest.RewardGroups.Values)
            {
                sb.AppendFormat("<rewards id=\"{0}\" type=\"{1}\">", group.Id, (int) group.Type);

                foreach (var reward in group.Rewards.Where(a => a.Result == QuestResult.Perfect))
                    sb.AppendFormat("<reward>* {0}</reward>", reward);

                sb.AppendFormat("</rewards>");
            }
            sb.AppendFormat("<desc>{0}</desc>", quest.Description);
            sb.AppendFormat("<values maxcount=\"{0}\" remaincount=\"{1}\" remaintime=\"{2}\" history=\"{3}\"/>",
                MaxAvailableJobs, RemainingJobs, remainingHours, History);
            sb.Append("</arbeit>");
        }
    }

    /// <summary>
    ///     Displays PTJ report window.
    /// </summary>
    public class DialogPtjReport : DialogElement
    {
        public DialogPtjReport(QuestResult result)
        {
            Result = result;
        }

        public QuestResult Result { get; set; }

        public override void Render(ref StringBuilder sb)
        {
            sb.AppendFormat("<arbeit_report result=\"{0}\"/>", (byte) Result);
        }
    }
}