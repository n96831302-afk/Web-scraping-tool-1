using System.Diagnostics;

static class Err {
  //處理例外的 Try 包裝器，讓你不用每次都寫 try-catch 就能捕捉錯誤並顯示訊息框：
  public static void Try(Action f) { try { f(); } catch (Exception ex) { us_ErrMsg(ex); } }

  /* 如果你覺得 lambda 太長，可以再短一點，C# 支援 method group，你可以再縮：加一個 helper：*/
  //this.Load, btn.Click 等 EventHandler 用
  public static EventHandler Wrap(Action<object, EventArgs> f) => (s, e) => Try(() => f(s, e));
  //this.FormClosing
  public static FormClosingEventHandler WrapFC(Action<object, FormClosingEventArgs> f) => (s, e) => Try(() => f(s, e));
  //dgv.CellContentClick
  public static DataGridViewCellEventHandler WrapCell(Action<object, DataGridViewCellEventArgs> f) => (s, e) => Try(() => f(s, e));
  //...其他事件類型的 Wrap 方法，太多了就不寫了，你需要哪個事件類型就加一個 Wrap 方法，裡面就是把對應的 EventArgs 換掉而已。

  /*copilot: 如果你不想每次都 new StackFrame(0, true)，可以把這個動作藏進 us_ErrMsg 自己裡面：*/
  public static void us_ErrMsg(Exception ex, object title = null) {
    var st = new StackTrace(ex, true);       // 用例外物件建立 StackTrace
    var f = st.GetFrame(0) ?? new StackFrame(0, true);
    var m = f.GetMethod();
    var sfrmName = m?.ReflectedType?.Name;
    var sSubName = m?.Name;

    string msg = $"{ex.Message}(程式：@{sfrmName}﹒{sSubName})";
    string caption = title?.ToString() ?? "錯誤";
    MessageBox.Show(msg, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
  }
}