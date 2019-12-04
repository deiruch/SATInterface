﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Management;

namespace SATInterface
{
    public class CryptoMiniSat : IDisposable
    {
        private IntPtr Handle;

        public CryptoMiniSat(int _threads = -1)
        {
            Handle = CryptoMiniSatNative.cmsat_new();

            if (_threads == -1)
                _threads = GetNumberOfPhysicalCores();

            if (_threads != 1)
                CryptoMiniSatNative.cmsat_set_num_threads(Handle, (uint)_threads);
        }

        private static int GetNumberOfPhysicalCores()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                return Environment.ProcessorCount;
            else
                //Code by Kevin Kibler
                //- http://stackoverflow.com/questions/1542213/how-to-find-the-number-of-cpu-cores-via-net-c
                using (var ms = new ManagementObjectSearcher("SELECT NumberOfCores FROM Win32_Processor"))
                    return ms.Get()
                        .OfType<ManagementBaseObject>()
                        .Sum(i => int.Parse(i["NumberOfCores"].ToString()));
        }

        public bool Solve(int[] _assumptions = null)
        {
            if(_assumptions==null || _assumptions.Length==0)
                return CryptoMiniSatNative.cmsat_solve(Handle) == CryptoMiniSatNative.c_lbool.L_TRUE;
            else
                return CryptoMiniSatNative.cmsat_solve_with_assumptions(Handle,
                    _assumptions.Select(v => v < 0 ? (-v - v - 2 + 1) : (v + v - 2)).ToArray(),
                    (IntPtr)_assumptions.Length) == CryptoMiniSatNative.c_lbool.L_TRUE;
        }

        public void AddVars(int _number)
        {
            CryptoMiniSatNative.cmsat_new_vars(Handle, (IntPtr)_number);
        }

        public bool AddClause(int[] _clause)
        {
            return CryptoMiniSatNative.cmsat_add_clause(Handle,
                _clause.Select(v => v < 0 ? (-v - v - 2 + 1) : (v + v - 2)).ToArray(),
                (IntPtr)_clause.Length);
        }

        public bool[] GetModel()
        {
            var model = CryptoMiniSatNative.cmsat_get_model(Handle);
            var bytes = new byte[(int)model.num_vals];
            Marshal.Copy(model.vals, bytes, 0, (int)model.num_vals);
            return bytes.Select(v => v== (byte)CryptoMiniSatNative.c_lbool.L_TRUE).ToArray();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                CryptoMiniSatNative.cmsat_free(Handle);
                Handle = IntPtr.Zero;

                disposedValue = true;
            }
        }

        ~CryptoMiniSat()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    public static class CryptoMiniSatNative
    {
        //https://github.com/msoos/cryptominisat/blob/master/src/cryptominisat_c.h.in

        //typedef struct slice_Lit { const c_Lit* vals; size_t num_vals; }
        //typedef struct slice_lbool { const c_lbool* vals; size_t num_vals; }

        public enum c_lbool : byte
        {
            L_TRUE = 0,
            L_FALSE = 1,
            L_UNDEF = 2
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct slice_lbool
        {
            public IntPtr vals;
            public IntPtr num_vals;
        }

        [DllImport("cryptominisat5win.dll")]
        public static extern bool cmsat_add_clause(IntPtr self, [In, MarshalAs(UnmanagedType.LPArray)] Int32[] lits, IntPtr num_lits);

        [DllImport("cryptominisat5win.dll")]
        public static extern bool cmsat_add_xor_clause(IntPtr self, [In, MarshalAs(UnmanagedType.LPArray)] UInt32[] vars, IntPtr num_vars, bool rhs);

        [DllImport("cryptominisat5win.dll")]
        public static extern void cmsat_free(IntPtr s);

        //[DllImport("cryptominisat5.exe")]
        //public static extern slice_Lit cmsat_get_conflict(SATSolver self);

        [DllImport("cryptominisat5win.dll")]
        public static extern slice_lbool cmsat_get_model(IntPtr self);

        [DllImport("cryptominisat5win.dll")]
        public static extern IntPtr cmsat_new();

        [DllImport("cryptominisat5win.dll")]
        public static extern void cmsat_new_vars(IntPtr self, IntPtr n);

        [DllImport("cryptominisat5win.dll")]
        public static extern UInt32 cmsat_nvars(IntPtr self);

        [DllImport("cryptominisat5win.dll")]
        public static extern void cmsat_set_num_threads(IntPtr self, UInt32 n);

        [DllImport("cryptominisat5win.dll")]
        public static extern c_lbool cmsat_solve(IntPtr self);

        [DllImport("cryptominisat5win.dll")]
        public static extern c_lbool cmsat_solve_with_assumptions(IntPtr self, [In, MarshalAs(UnmanagedType.LPArray)] Int32[] assumptions, IntPtr num_assumptions);
    }
}
