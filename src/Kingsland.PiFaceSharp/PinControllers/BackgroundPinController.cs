﻿using System;
using System.Threading;

namespace Kingsland.PiFaceSharp.PinControllers
{

    /// <summary>
    /// Implements a common base for a pin controller that is run on a
    /// separate thread. This allows the background pin controller to
    /// perform its behaviour without the need for the main program
    /// thread to manage the state of the pin.
    /// </summary>
    public abstract class BackgroundPinController : PinControllerBase
    {

        public enum BackgroundPinControllerStatus
        {
            Stopped,
            Stopping,
            Running
        }

        #region Fields

        private readonly object _lockObject = new object();

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new background pin controller that is bound to the specified
        /// PiFace Digital device.
        /// </summary>
        /// <param name="piface"></param>
        protected BackgroundPinController(IPiFaceDevice piface)
            : base(piface)
        {
        }

        #endregion

        #region Abstract Members

        /// <summary>
        /// 
        /// </summary>
        protected abstract void Execute();

        #endregion

        #region Properties

        /// <summary>
        /// 
        /// </summary>
        private Thread WorkerThread
        {
            get;
            set;
        }

        public BackgroundPinControllerStatus Status
        {
            get;
            private set;
        }

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        public void Start()
        {
            lock (_lockObject)
            {
                switch (this.Status)
                {
                    case BackgroundPinControllerStatus.Stopped:
                        Console.WriteLine("starting pin controller thread");
                        this.Status = BackgroundPinControllerStatus.Running;
                        this.WorkerThread = new Thread(this.Execute)
                        {
                            IsBackground = true
                        };
                        this.WorkerThread.Start();
                        break;
                    default:
                        throw new InvalidOperationException(
                            string.Format("Cannot start a controller with the status '{0}'", this.Status));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Stop()
        {
            lock (_lockObject)
            {
                switch (this.Status)
                {
                    case BackgroundPinControllerStatus.Running:
                        Console.WriteLine("stopping pin controller thread");
                        this.Status = BackgroundPinControllerStatus.Stopping;
                        while (this.WorkerThread.IsAlive)
                        {
                            Thread.Sleep(100);
                        }
                        this.WorkerThread = null;
                        this.Status = BackgroundPinControllerStatus.Stopped;
                        break;
                    default:
                        throw new InvalidOperationException(
                            string.Format("Cannot stop a controller with the status '{0}'", this.Status));
                }
            }
        }

        #endregion

    }

}
