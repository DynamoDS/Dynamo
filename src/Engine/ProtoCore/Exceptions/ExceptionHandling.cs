using System.Collections.Generic;
using System.Diagnostics;
using ProtoCore.Utils;

namespace ProtoCore.Exceptions
{
    public class ExceptionContext
    {
        public int typeUID;
        public int pc;
        public int codeBlockId;
        public int functionScope;
        public int classScope;
    }

    // TODO: Need more rafactorings...
    public class CatchHandler
    {
        public int FilterTypeUID { get; set; }
        public int Entry { get; set; }
        public int Index { get; set; }

        public bool CatchIt(int exceptionTypeUID)
        {
            return FilterTypeUID == exceptionTypeUID;
        }

        public CatchHandler()
        {
            FilterTypeUID = (int)ProtoCore.PrimitiveType.kTypeVar;
            Index = ProtoCore.DSASM.Constants.kInvalidIndex;
        }
    }

    // Each try...catch block is represented in an ExceptionHandler data 
    // structure, as a try...catch block may have multiple catch blocks, there 
    // is a list of CatchHandler in an ExceptionHandler instance. As it is 
    // allowed to nest the other try...catch block inside a try...catch block, 
    // and the exception will be propagated from inside out, an ExceptionHandler 
    // has a propery TryLevel to indicate its nested level and a propery 
    // ParentIndex to point to its upper try..catch block.
    //
    // Each language block and function definition has a data structure called
    // ExceptionRegistration which keeps all try...catch blocks that defined in 
    // its scope. The code gen will create an ExceptionRegistration instance 
    // when it is entering a new scope. The sample code shows an example:
    //
    //
    // E.g.
    // 
    //    def foo()
    //    {
    //        ...
    //        try <----------------------+
    //        {                          |
    //            ...                    |
    //            try { ...  }  <--------+-----+
    //            catch (...) { ... }    |     |
    //            catch (...) { ... }    |     |
    //        }                          |     |
    //        catch (...) { ...  }       |     |
    //        ...                        |     |
    //        try {...} <----------------+-----+-----+
    //        catch { ... }              |     |     |
    //    }                              |     |     |
    //                                   |     |     |
    //    List<ExceptionHandler>:        |     |     |
    //                                   |     |     |
    //    +-------------------------+    |     |     |
    //    | ExceptionHandler        |    |     |     |
    //    |     * Index = 0      o--+----+     |     |
    //    |     * ParentIndex = -1  |          |     |
    //    |     * TryLevel = 0      |          |     |
    //    |     * CatchHandlers     |          |     |
    //    |     * ...               |<---+     |     |
    //    +-------------------------+    |     |     |
    //                                   |     |     |
    //    +-------------------------+    |     |     |
    //    | ExceptionHandler        |    |     |     |
    //    |     * Index = 1      o--+----+-----+     |    
    //    |     * ParentIndex = 0o--+----+           |
    //    |     * TryLevel = 1      |                |
    //    |     * CatchHandlers     |                |
    //    |     * ...               |                |
    //    +-------------------------+                |
    //                                               |
    //    +-------------------------+                |    
    //    | ExceptionHandler        |                |
    //    |     * Index = 2      o--+----------------+    
    //    |     * ParentIndex = -1  |
    //    |     * TryLevel = 0      |
    //    |     * CatchHandlers     |
    //    |     * ...               |
    //    +-------------------------+    

    public class ExceptionHandler
    {
        // An ExceptionHandler instance is stored in an ExceptionRegistion instance
        public ExceptionRegistration Registration { get; set; }

        // TryLevel to indicate its level
        public int TryLevel { get; set; }

        // The index of the ExceptionHandler instance for its upper level of 
        // try...catch block in the ExceptionRegistration instance's Handlers 
        public int ParentIndex { get; set;}

        // Its index in ExceptionRegistration's Handlers 
        public int HandlerIndex { get; set; }

        // Start pc of try block
        public int StartPc { get; set; }

        // End pc of try block
        public int EndPc { get; set; }

        // Catch blocks
        public List<CatchHandler> CatchHandlers { get; set; }

        public ExceptionHandler()
        {
            TryLevel = 0;
            ParentIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            HandlerIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            CatchHandlers = new List<CatchHandler>();
        }

        public bool CanHandleIt(int exceptionType, out int catchHandlerIndex)
        {
            catchHandlerIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            foreach (var catchHandler in CatchHandlers)
            {
                if (catchHandler.CatchIt(exceptionType))
                {
                    catchHandlerIndex = catchHandler.Index;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// If a pc is in my scope?
        /// </summary>
        /// <param name="pc"></param>
        /// <returns></returns>
        public bool IsInScope(int pc)
        {
            return (StartPc <= pc) && (EndPc >= pc);
        }

        /// <summary>
        /// Add a catch handler
        /// </summary>
        /// <param name="catchHandler"></param>
        public void AddCatchHandler(CatchHandler catchHandler)
        {
            CatchHandlers.Add(catchHandler);
            catchHandler.Index = CatchHandlers.Count - 1;
        }

        /// <summary>
        /// Get the corresponding catch handler for specified exception
        /// </summary>
        /// <param name="exceptionType"></param>
        /// <returns></returns>
        public CatchHandler GetCatchHandler(int exceptionType)
        {
            int catchHandlerIndex;
            if (CanHandleIt(exceptionType, out catchHandlerIndex))
            {
                return CatchHandlers[catchHandlerIndex];
            }
            return null;
        }
    }

    // Each language block and function will have an ExceptionRegistration slot.
    // All try-catch exception handlers defined in that language block and
    // function will be put in Handler list.
    //
    // Each exception registration instance is identified by three parameters:
    //     code block id, function scope, class scope.
    // 
    // All exception registrations chained together, therefore if an exception
    // doesn't get handled in current scope, we will follow exception registration
    // chain to trace back to its parent scope, repeat this process until either
    // a proper exception handler is found or the default system exception handler
    // is called.
    //
    // E.g.  
    //
    // [Associative]            -- ExceptionRegistration 2
    // {
    //     ...
    //     def foo()            -- ExceptionRegistration 3
    //     {
    //         ...
    //         [Imperative]     -- ExceptionRegistration 4
    //         {
    //             ...          <-- current pc
    //         }
    //     }
    //     foo();               <-- call stack from here
    //     
    // List<ExceptionRegistration> Registrations:
    //
    // 0 is system default exception registration instance
    // 1 is exception registration instance for the top-most associative language block (id = 0)
    //
    //         2                          3                             4
    // +-------------------------+-------------------------+-------------------------+
    // | ExceptionRegistration 1 | ExceptionRegistration 2 | ExceptionRegistration 2 |
    // | +---------------------+ | +---------------------+ | +---------------------+ |
    // | | CodeBlockId = 0     | | | CodeBlockId = 1     | | | CodeBlockId = 2     | |
    // | +---------------------+ | +---------------------+ | +---------------------+ |
    // | | FunctionScope = -1  | | | FunctionScope = 0   | | | FunctionScope = -1  | |
    // | +---------------------+ | +---------------------+ | +---------------------+ |
    // | | ClassScope = -1     | | | ClassScope = -1     | | | ClassScope = -1     | |
    // | +---------------------+ | +---------------------+ | +---------------------+ |
    // | | ParentIndex = 1     | | | ParentIndex = 2     | | | ParentIndex = 3     | |
    // | +---------------------+ | +---------------------+ | +---------------------+ |
    // | | Index = 2           | | | Index = 3           | | | Index = 4           | |
    // | +---------------------+ | +---------------------+ | +---------------------+ |
    // | | Handlers            | | | Handlers            | | | Handlers            | |
    // | +---------------------+ | +---------------------+ | +---------------------+ |
    // +-------------------------+-------------------------+-------------------------+
    // 
    // NOTE 1: ExceptionRegistration.ParentIndex will be dynamically changed at
    // run-time. 
    //
    // NOTE 2: It is possible that for a language block whose function scope and
    // class scope is valid. For example, that language block is embedded in a
    // function definition.   -- Yu Ke
    // 
    public class ExceptionRegistration
    {
        public int CodeBlockId { get; set; }
        public int FunctionScope { get; set; }
        public int ClassScope { get; set; }
        
        // Its index in registration table
        public int RegistrationTableIndex { get; set; }

        // Its parent scope's registration index in registration table
        // This value is only set/updated at run-time
        public int ParentIndex { get; set; }

        // From where that we swithc to this scope? 
        // This value is only set/updated at run-time
        public int LastPc { get; set; }

        // All try-catch blocks in this scope
        public List<ExceptionHandler> Handlers { get; set; } 

        public ExceptionRegistration(int codeBlockId, int functionScope, int classScope)
        {
            CodeBlockId = codeBlockId;
            FunctionScope = functionScope;
            ClassScope = classScope;

            ParentIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            LastPc = ProtoCore.DSASM.Constants.kInvalidIndex;

            RegistrationTableIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            Handlers = new List<ExceptionHandler>();
        }

        // Given an exception, if the exception will be handled by this 
        // exception registration. 
        public bool HandleIt(int pc, int exceptionTypeUID, out int handlerIndex)
        {
            // As during the code gen process, the inner try-block is added to 
            // Handlers later than its parent try-block, we can go through the 
            // list to find out the last try-block where pc is in its scope
            handlerIndex = ProtoCore.DSASM.Constants.kInvalidIndex;

            // Get the corresponding try-catch block
            ExceptionHandler targetHandler = null;
            foreach (var handler in Handlers)
            {
                if (handler.IsInScope(pc))
                {
                    targetHandler = handler;
                }
            }

            // Get the corresponding catch block
            int catchHandlerIndex;
            if (targetHandler != null && targetHandler.CanHandleIt(exceptionTypeUID, out catchHandlerIndex))
            {
                handlerIndex = targetHandler.HandlerIndex;
                return true;
            }
            return false;
        }

        public void Add(ExceptionHandler newHandler)
        {
            Handlers.Add(newHandler);
            newHandler.Registration = this;
            newHandler.HandlerIndex = Handlers.Count - 1;

            // If a hander's TryLevel == 1, then it is a top-level try block, 
            // we don't need to set its parent index; otheriwse we search its 
            // parent try-block in Handlers in reverse order.
            if (newHandler.TryLevel > 1)
            {
                for (int i = Handlers.Count - 2; i >= 0; --i)
                {
                    if (Handlers[i].TryLevel == newHandler.TryLevel - 1)
                    {
                        newHandler.ParentIndex = i;
                        return;
                    }
                }
                Validity.Assert(false, "Cannot find try-block's parent block");
            }
        }
    }

    public class ExceptionRegistrationTable
    {
        public List<ExceptionRegistration> Registrations { get; set; }

        public ExceptionRegistrationTable()
        {
            Registrations = new List<ExceptionRegistration>();
        }

        /// <summary>
        /// Get the index of an exception registration instance in Registrations
        /// </summary>
        /// <param name="codeBlockId"></param>
        /// <param name="functionScope"></param>
        /// <param name="classScope"></param>
        /// <returns></returns>
        public int IndexOf(int codeBlockId, int functionScope, int classScope)
        {
            foreach (var reg in Registrations)
            {
                if (reg.CodeBlockId == codeBlockId &&
                    reg.ClassScope == classScope &&
                    reg.FunctionScope == functionScope)
                {
                    return reg.RegistrationTableIndex;
                }
            }
            return ProtoCore.DSASM.Constants.kInvalidIndex;
        }

        /// <summary>
        /// Get an exception registration instance for specified scope.
        /// </summary>
        /// <param name="codeBlockId"></param>
        /// <param name="functionScope"></param>
        /// <param name="classScope"></param>
        /// <returns></returns>
        public ExceptionRegistration GetExceptionRegistration(int codeBlockId, int functionScope, int classScope)
        {
            int index = IndexOf(codeBlockId, functionScope, classScope);
            if (index == ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                return null;
            }
            else
            {
                return Registrations[index];
            }
        }

        /// <summary>
        /// Register an ExceptionRegistration instance for a scope. It is identified
        /// by three parameters: code block id, function index and class index.
        /// </summary>
        /// <param name="codeBlockId"></param>
        /// <param name="functionScope"></param>
        /// <param name="classScope"></param>
        /// <returns></returns>
        public ExceptionRegistration Register(int codeBlockId, int functionScope, int classScope)
        {
            int index = IndexOf(codeBlockId, functionScope, classScope);
            if (index != ProtoCore.DSASM.Constants.kInvalidIndex)
            {
                return Registrations[index];
            }

            ExceptionRegistration registration = new ExceptionRegistration(codeBlockId, functionScope, classScope);
            Registrations.Add(registration);
            registration.RegistrationTableIndex = Registrations.Count - 1;

            // reg.ParentIndex will be set at run-time because at this moment
            // we have no idea about its calling context
            return registration;
        }

        /// <summary>
        /// Walk through chained exception registration instances to find out
        /// the corresponding exception handler. Returns null if cannot find
        /// the handler.
        /// </summary>
        /// <param name="context">Exception context</param>
        /// <returns></returns>
        public ExceptionHandler FindExceptionHandler(ExceptionContext context)
        {
            int regIndex = IndexOf(context.codeBlockId, context.functionScope, context.classScope);
            Validity.Assert(regIndex != ProtoCore.DSASM.Constants.kInvalidIndex);

            ExceptionRegistration registration = Registrations[regIndex];
            int pc = context.pc;
            while (true)
            {
                int handlerIndex;
                if (registration.HandleIt(pc, context.typeUID, out handlerIndex))
                {
                    Validity.Assert(handlerIndex != ProtoCore.DSASM.Constants.kInvalidIndex);
                    return registration.Handlers[handlerIndex];
                }
                else
                {
                    int parentIndex = registration.ParentIndex;
                    if (parentIndex == ProtoCore.DSASM.Constants.kInvalidIndex)
                    {
                        break;
                    }
                    else
                    {
                        pc = registration.LastPc;
                        registration = Registrations[parentIndex];
                    }
                }
            }
            return null;
        }
    }

    public interface IFirstHandExceptionObserver
    {
        void Notify(ExceptionContext context);
    }

    public class ExceptionHandlingManager
    {
        public ExceptionRegistration SystemRegistration;
        public ExceptionRegistration TopMostAssocBlockRegistration;
        public ExceptionRegistration CurrentActiveRegistration { get; set;}

        public ExceptionRegistrationTable ExceptionTable { get; set;}

        public ExceptionContext Context { get; set;}
        public ExceptionHandler TargetHandler { get; set;}
        public bool IsStackUnwinding { get; set;}

        private List<IFirstHandExceptionObserver> Observers { get; set; }

        public ExceptionHandlingManager()
        {
            ExceptionTable = new ExceptionRegistrationTable();

            int invalidIndex = ProtoCore.DSASM.Constants.kInvalidIndex;
            // System default exception registration
            SystemRegistration = ExceptionTable.Register(invalidIndex, invalidIndex, invalidIndex); 
            SystemRegistration.ParentIndex = ProtoCore.DSASM.Constants.kInvalidIndex;

            // Exception registration for the top most associative code block.
            //
            // Why we need to manually add this one even when code gen is compiling 
            // the top-most language node the exception registration for that 
            // language code block will be added?
            // 
            // The reason is ParentIndex, which will be updated dynamcailly at
            // run-time, i.e., it should point to the index of exception registration
            // for the upper scope. Normally, we will set this value either at
            // OpCode.Bounce or at OpCode.CallR. But for the top-most language
            // block, it doesnt start from there, so here we manully set its
            // ParentIndex to the default system exception registration instance.
            //
            // If we can consolidate the updating code at OpCode.Bounce and at
            // OpCode.CallR, then it is not necessary to manually add this one.
            // (The reason that we have to do that is because we need the value 
            // of pc in the upper scope, i.e., where does the execution jump to 
            // other function call or language block; otherwise Execute() is a 
            // ideal place to put that updating code). Should refactor this piece 
            // of code for that case.  -- Yu Ke
            TopMostAssocBlockRegistration = ExceptionTable.Register(0, invalidIndex, invalidIndex);
            TopMostAssocBlockRegistration.ParentIndex = SystemRegistration.RegistrationTableIndex;

            CurrentActiveRegistration = TopMostAssocBlockRegistration;

            IsStackUnwinding = false;
            TargetHandler = null;
            Context = null;

            Observers = new List<IFirstHandExceptionObserver>();
        }

        /// <summary>
        /// To switch scope, so need to set current active exception registration 
        /// for current scope. Each exception registration instance is identified
        /// by three parameter: block id, function index and class index. PC is
        /// needed so that the executive knows where the switch of context happens,
        /// PC values will be used to determine which excpetion handler will handle
        /// this exception.
        /// </summary>
        /// <param name="blockId">Block ID</param>
        /// <param name="functionIdex">Fucntion index</param>
        /// <param name="classIndex">Class index</param>
        /// <param name="pc">PC</param>
        public void SwitchContextTo(int blockId, int functionIdex, int classIndex, int pc)
        {
            int parentIndex = CurrentActiveRegistration.RegistrationTableIndex;
            ExceptionRegistration reg = ExceptionTable.GetExceptionRegistration(blockId, functionIdex, classIndex);
            if (reg != null)
            {
                reg.ParentIndex = parentIndex;
                CurrentActiveRegistration = reg;
                CurrentActiveRegistration.LastPc = pc;
            }
        }

        /// <summary>
        /// Walking through the exception handler chain to find the proper 
        /// exception handler. Also notify the observers who are keen on being 
        /// notified when an exception is happening. 
        /// </summary>
        /// <param name="context">Exception context</param>
        public void HandleFirstHandException(ExceptionContext context)
        {
            ExceptionHandler exceHandler = ExceptionTable.FindExceptionHandler(context);
            if (exceHandler != null)
            {
                TargetHandler = exceHandler;
            }
            Context = context;
            IsStackUnwinding = true;

            // Anybody wants to be notified before going to exception handler
            // or stack unwinding?
            foreach (IFirstHandExceptionObserver observer in Observers)
            {
                observer.Notify(context);
            }
        }

        public ExceptionRegistration Register(int blockId, int procIndex, int classIndex)
        {
            ExceptionRegistration registration = ExceptionTable.GetExceptionRegistration(blockId, procIndex, classIndex);
            if (registration == null)
            {
                registration = this.ExceptionTable.Register(blockId, procIndex, classIndex);
                Validity.Assert(registration != null);
            }
            return registration;
        }

        /// <summary>
        /// If the exception can be handled in current scope, return true and 
        /// set pc which is where the executive will jump to. Otherwise return
        /// false.
        /// </summary>
        /// <param name="pc">New pc value</param>
        /// <returns></returns>
        public bool CanHandleIt(ref int pc)
        {
            if (!IsStackUnwinding || TargetHandler == null || Context == null)
                return false;

            // Note there is always an instance of ExceptionRegistration 
            // associated with a scope -- either function defintion or a language 
            // block -- and that instance of ExceptionRegistration associated 
            // with current scope is called CurrentActiveRegistration which is
            // dynamically upated when switching scopes.
            if (TargetHandler.Registration == CurrentActiveRegistration)
            {
                CatchHandler catchHandler = TargetHandler.GetCatchHandler(Context.typeUID);
                if (catchHandler != null)
                {
                    pc = catchHandler.Entry;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Notify the exception handling manager that this exception has been
        /// handled, so no more stack unwinding and exception context will be 
        /// cleaned up.
        /// </summary>
        public void SetHandled()
        {
            IsStackUnwinding = false;
            TargetHandler = null;
            Context = null;
        }
    }
}
