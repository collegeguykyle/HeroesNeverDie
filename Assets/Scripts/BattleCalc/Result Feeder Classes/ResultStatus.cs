


using System.Collections.Generic;

public class ResultStatus : ActionResult
{
    public Status status;
    public int stacks;

    public ResultStatus (Status status, int stacks)
    {
        this.status = status;
        this.stacks = stacks;
    }
    public ResultStatus() { }
}


public interface IApplyStatus
{
    public abstract List<Status> StatusToApply { get; }
}