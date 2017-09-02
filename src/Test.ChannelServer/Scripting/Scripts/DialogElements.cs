// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Scripting.Scripts;
using Aura.Mabi.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Aura.Tests.Channel.Scripting.Scripts
{
	public class DialogElementTests
	{
		[Fact]
		public void Rendering()
		{
			var sb = new StringBuilder();

			new DialogText("test {0}", 123).Render(ref sb);
			Assert.Equal("test 123", sb.ToString());
			sb.Clear();

			new DialogPortrait("foobar").Render(ref sb);
			Assert.Equal("<npcportrait name='foobar' />", sb.ToString());
			sb.Clear();

			new DialogTitle("foobar").Render(ref sb);
			Assert.Equal("<title name='foobar' />", sb.ToString());
			sb.Clear();

			new DialogTitle(null).Render(ref sb);
			Assert.Equal("<title name='NONE' />", sb.ToString());
			sb.Clear();

			new DialogHotkey("foobar").Render(ref sb);
			Assert.Equal("<hotkey name='foobar' />", sb.ToString());
			sb.Clear();

			new DialogButton("Foobar").Render(ref sb);
			Assert.Equal("<button title='Foobar' keyword='@foobar' />", sb.ToString());
			sb.Clear();

			new DialogButton("Foobar", "@foo").Render(ref sb);
			Assert.Equal("<button title='Foobar' keyword='@foo' />", sb.ToString());
			sb.Clear();

			new DialogButton("Foobar", "@foo", "asdf").Render(ref sb);
			Assert.Equal("<button title='Foobar' keyword='@foo' onframe='asdf' />", sb.ToString());
			sb.Clear();

			new DialogBgm("foobar.mp3").Render(ref sb);
			Assert.Equal("<music name='foobar.mp3'/>", sb.ToString());
			sb.Clear();

			new DialogImage("foobar").Render(ref sb);
			Assert.Equal("<image name='foobar' />", sb.ToString());
			sb.Clear();

			new DialogImage("foobar", true).Render(ref sb);
			Assert.Equal("<image name='foobar' local='true' />", sb.ToString());
			sb.Clear();

			new DialogImage("foobar", true, 100, 200).Render(ref sb);
			Assert.Equal("<image name='foobar' local='true' width='100' height='200' />", sb.ToString());
			sb.Clear();

			new DialogList("foobar", new DialogButton("test1")).Render(ref sb);
			Assert.Equal("<listbox page_size='1' title='foobar' cancel='@end'><button title='test1' keyword='@test1' /></listbox>", sb.ToString());
			sb.Clear();

			new DialogList("foobar", 20, "@foo", new DialogButton("test1"), new DialogButton("test2")).Render(ref sb);
			Assert.Equal("<listbox page_size='20' title='foobar' cancel='@foo'><button title='test1' keyword='@test1' /><button title='test2' keyword='@test2' /></listbox>", sb.ToString());
			sb.Clear();

			new DialogInput("Foobar", "...", 21, false).Render(ref sb);
			Assert.Equal("<inputbox title='Foobar' caption='...' max_len='21' allow_cancel='false' />", sb.ToString());
			sb.Clear();

			new DialogAutoContinue(20).Render(ref sb);
			Assert.Equal("<autopass duration='20'/>", sb.ToString());
			sb.Clear();

			new DialogFaceExpression("good").Render(ref sb);
			Assert.Equal("<face name='good'/>", sb.ToString());
			sb.Clear();

			new DialogMovie("foobar.wmv", 500, 300, true).Render(ref sb);
			Assert.Equal("<movie name='foobar.wmv' width='500' height='300' loop='true' />", sb.ToString());
			sb.Clear();

			new DialogMinimap(true, false, true).Render(ref sb);
			Assert.Equal("<openminimap zoom='true' max_size='false' center='true' />", sb.ToString());
			sb.Clear();

			new DialogShowPosition(1, 20000, 30000, 5000).Render(ref sb);
			Assert.Equal("<show_position region='1' pos='20000 30000' remainingtime='5000' />", sb.ToString());
			sb.Clear();

			new DialogShowDirection(100, 200, 90).Render(ref sb);
			Assert.Equal("<show_dir pos='100 200' pitch='90' />", sb.ToString());
			sb.Clear();

			new DialogSetDefaultName("Foobar").Render(ref sb);
			Assert.Equal("<defaultname name='Foobar' />", sb.ToString());
			sb.Clear();

			new DialogSelectItem("Foobar", "...", "/foobar/").Render(ref sb);
			Assert.Equal("<selectitem title='Foobar' caption='...' stringid='/foobar/' />", sb.ToString());
			sb.Clear();

			//new DialogPtjDesc(...).Render(ref sb);
			//Assert.Equal("", sb.ToString());
			//sb.Clear();

			new DialogPtjReport(QuestResult.Mid).Render(ref sb);
			Assert.Equal("<arbeit_report result=\"1\"/>", sb.ToString());
			sb.Clear();
		}

		[Fact]
		public void ElementComposition()
		{
			var sb = new StringBuilder();
			var elements = new DialogElement();

			elements.Add(new DialogButton("Foobar1"));
			elements.Render(ref sb);
			Assert.Equal("<button title='Foobar1' keyword='@foobar1' />", sb.ToString());
			sb.Clear();

			elements.Add(new DialogButton("Foobar2"));
			elements.Render(ref sb);
			Assert.Equal("<button title='Foobar1' keyword='@foobar1' /><button title='Foobar2' keyword='@foobar2' />", sb.ToString());
			sb.Clear();

			elements.Insert(1, new DialogButton("Foobar3"));
			elements.Render(ref sb);
			Assert.Equal("<button title='Foobar1' keyword='@foobar1' /><button title='Foobar3' keyword='@foobar3' /><button title='Foobar2' keyword='@foobar2' />", sb.ToString());
			sb.Clear();

			elements.Replace(0, new DialogButton("Foobar4"));
			elements.Render(ref sb);
			Assert.Equal("<button title='Foobar4' keyword='@foobar4' /><button title='Foobar3' keyword='@foobar3' /><button title='Foobar2' keyword='@foobar2' />", sb.ToString());
			sb.Clear();

			elements.Insert(0, new DialogButton("Foobar5"));
			elements.Render(ref sb);
			Assert.Equal("<button title='Foobar5' keyword='@foobar5' /><button title='Foobar4' keyword='@foobar4' /><button title='Foobar3' keyword='@foobar3' /><button title='Foobar2' keyword='@foobar2' />", sb.ToString());
			sb.Clear();

			elements.Insert(99, new DialogButton("Foobar6"));
			elements.Render(ref sb);
			Assert.Equal("<button title='Foobar5' keyword='@foobar5' /><button title='Foobar4' keyword='@foobar4' /><button title='Foobar3' keyword='@foobar3' /><button title='Foobar2' keyword='@foobar2' /><button title='Foobar6' keyword='@foobar6' />", sb.ToString());
			sb.Clear();

			elements = new DialogElement();

			elements.Insert(1, new DialogButton("Foobar7"));
			elements.Render(ref sb);
			Assert.Equal("<button title='Foobar7' keyword='@foobar7' />", sb.ToString());
			sb.Clear();

			elements = new DialogElement();

			elements.Replace(0, new DialogButton("Foobar8"));
			elements.Render(ref sb);
			Assert.Equal("<button title='Foobar8' keyword='@foobar8' />", sb.ToString());
			sb.Clear();

			elements = new DialogElement("foobar, <username/>!", new DialogButton("Foobar9"), new DialogButton("Foobar10"));

			elements.Render(ref sb);
			Assert.Equal("foobar, <username/>!<button title='Foobar9' keyword='@foobar9' /><button title='Foobar10' keyword='@foobar10' />", sb.ToString());
			sb.Clear();

			elements.Replace(0, "barfoo?");
			elements.Insert(1, new DialogButton("Foobar11"));
			elements.Render(ref sb);
			Assert.Equal("barfoo?<button title='Foobar11' keyword='@foobar11' /><button title='Foobar9' keyword='@foobar9' /><button title='Foobar10' keyword='@foobar10' />", sb.ToString());
			sb.Clear();
		}
	}
}
