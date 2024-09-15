


using System.Collections.Generic;

public class ResultStatus : ActionResult
{
    public Status status;
    public int stacksChange;
    public int newStacksTotal;
    
    public bool newStatus = false; //set true if unit does not currently have any stacks of this status and replay should add an icon
    public bool removeStatus = false; //set true if status is gone now and replay should remove its icon

    public ResultStatus (Status status, int stacks)
    {
        this.status = status;
        this.stacksChange = stacks;
    }
    public ResultStatus() { }
}


public interface IApplyStatus
{
    public abstract List<Status> StatusToApply { get; }
}