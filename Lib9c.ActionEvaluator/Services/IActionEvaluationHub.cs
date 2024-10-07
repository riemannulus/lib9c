namespace Lib9c.ActionEvaluator.Services;

public interface IActionEvaluationHub
{
    Task JoinAsync(string addressHex);
    Task LeaveAsync();
    Task BroadcastRenderAsync(byte[] outputStates);
    Task BroadcastUnrenderAsync(byte[] outputStates);
    Task BroadcastRenderBlockAsync(byte[] oldTip, byte[] newTip);
    Task ReportReorgAsync(byte[] oldTip, byte[] newTip, byte[] branchpoint);
    Task ReportReorgEndAsync(byte[] oldTip, byte[] newTip, byte[] branchpoint);
    Task ReportExceptionAsync(int code, string message);
}
