﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LoveNes
{
    public partial class CPU
    {
        public enum OpCodeStatus : byte
        {
            None,

            Interrupt_1,
            Interrupt_2,
            Interrupt_3,
            Interrupt_4,
            Interrupt_5,
            Interrupt_6,

            Relative_Jump,

            BNE_1_Relative,

            BPL_1_Relative,

            JSR_1_Absolute,
            JSR_2_Absolute,
            JSR_3_Absolute,

            BIT_1_Absolute,

            RTI_1_Implied,
            RTI_2_Implied,
            RTI_3_Implied,
            RTI_4_Implied,
            RTI_5_Implied,

            JMP_1_Absolute,

            RTS_1_Implied,
            RTS_2_Implied,
            RTS_3_Implied,
            RTS_4_Implied,
            RTS_5_Implied,

            ADC_1_Addressing_Immediate,
            ADC_1_Addressing_ZeroPage,
            ADC_1_Addressing_ZeroPageX,
            ADC_1_Addressing_Absolute,
            ADC_1_Addressing_AbsoluteX,
            ADC_1_Addressing_AbsoluteY,
            ADC_1_Addressing_IndirectX,
            ADC_1_Addressing_IndirectY,
            ADC_2,

            AND_1_Immediate,

            SEI_1_Implied,

            TXA_1_Implied,

            STY_1_Absolute,

            STA_1_ZeroPage,
            STA_1_ZeroPageX,
            STA_1_Absolute,
            STA_1_AbsoluteX,

            STX_1_ZeroPage,
            STX_1_Absolute,

            TXS_1_Implied,

            LDX_1_Immediate,

            LDY_1_Immediate,

            TAX_1_Implied,

            TAY_1_Implied,

            LDA_1_Immediate,
            LDA_1_Absolute,
            LDA_1_AbsoluteX,
            LDA_1_IndirectY,
            LDA_1_ZeroPage,

            CMP_1_ZeroPage,
            CMP_1_Absolute,

            CLD_1_Implied,

            CPX_1_Immediate,

            INC_1_ZeroPage,

            INX_1_Implied,

            INY_1_Implied,

            DEX_1_Implied,

            BEQ_1_Relative
        }

        private (MicroCode nextMicroCode, OpCodeStatus nextOpCodeStatus) ExecuteOpCode(OpCodeStatus code)
        {
            switch (code)
            {
                case OpCodeStatus.Interrupt_1:
                    _addressState.ResultA = (byte)(Registers.PC >> 8);
                    return (MicroCode.Push, OpCodeStatus.Interrupt_2);
                case OpCodeStatus.Interrupt_2:
                    _addressState.ResultA = (byte)Registers.PC;
                    return (MicroCode.Push, OpCodeStatus.Interrupt_3);
                case OpCodeStatus.Interrupt_3:
                    _addressState.ResultA = Status.Value;
                    return (MicroCode.Push, OpCodeStatus.Interrupt_4);
                case OpCodeStatus.Interrupt_4:
                    _addressState.MemoryAddress = _interruptVectors[(byte)_interruptType.Value];
                    _masterClient.Read(_addressState.MemoryAddress);
                    _addressState.ResultA = _masterClient.Value;
                    return (MicroCode.Nop, OpCodeStatus.Interrupt_5);
                case OpCodeStatus.Interrupt_5:
                    _addressState.MemoryAddress++;
                    _masterClient.Read(_addressState.MemoryAddress);
                    _addressState.ResultB = _masterClient.Value;
                    return (MicroCode.Nop, OpCodeStatus.Interrupt_6);
                case OpCodeStatus.Interrupt_6:
                    _addressState.MemoryAddress = (ushort)((_addressState.ResultB << 8) | _addressState.ResultA);
                    Status.I = true;
                    _interruptType = null;
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.PC, AddressOperation.None, false);
                    return (MicroCode.Addressing, OpCodeStatus.None);

                case OpCodeStatus.Relative_Jump:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.PC, AddressOperation.None, false);
                    return (MicroCode.Relative, OpCodeStatus.None);

                case OpCodeStatus.SEI_1_Implied:
                    return (MicroCode.SEI, OpCodeStatus.None);
                case OpCodeStatus.CLD_1_Implied:
                    return (MicroCode.CLD, OpCodeStatus.None);

                case OpCodeStatus.STY_1_Absolute:
                    _addressState.Set(AddressOperand.Y, AddressOperand.None, AddressOperand.Memory, AddressOperation.None, false);
                    return (MicroCode.Absolute_1, OpCodeStatus.None);

                case OpCodeStatus.STA_1_ZeroPage:
                    _addressState.Set(AddressOperand.A, AddressOperand.None, AddressOperand.Memory, AddressOperation.None, false);
                    return (MicroCode.ZeroPage_1, OpCodeStatus.None);
                case OpCodeStatus.STA_1_ZeroPageX:
                    _addressState.Set(AddressOperand.A, AddressOperand.None, AddressOperand.Memory, AddressOperation.None, false);
                    return (MicroCode.ZeroPageX_1, OpCodeStatus.None);
                case OpCodeStatus.STA_1_AbsoluteX:
                    _addressState.Set(AddressOperand.A, AddressOperand.None, AddressOperand.Memory, AddressOperation.None, false);
                    return (MicroCode.AbsoluteX_1, OpCodeStatus.None);
                case OpCodeStatus.STA_1_Absolute:
                    _addressState.Set(AddressOperand.A, AddressOperand.None, AddressOperand.Memory, AddressOperation.None, false);
                    return (MicroCode.Absolute_1, OpCodeStatus.None);

                case OpCodeStatus.STX_1_ZeroPage:
                    _addressState.Set(AddressOperand.X, AddressOperand.None, AddressOperand.Memory, AddressOperation.None, false);
                    return (MicroCode.ZeroPage_1, OpCodeStatus.None);
                case OpCodeStatus.STX_1_Absolute:
                    _addressState.Set(AddressOperand.X, AddressOperand.None, AddressOperand.Memory, AddressOperation.None, false);
                    return (MicroCode.Absolute_1, OpCodeStatus.None);

                case OpCodeStatus.LDY_1_Immediate:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.Y, AddressOperation.None, true);
                    return (MicroCode.Immediate, OpCodeStatus.None);

                case OpCodeStatus.LDX_1_Immediate:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.X, AddressOperation.None, true);
                    return (MicroCode.Immediate, OpCodeStatus.None);

                case OpCodeStatus.TAX_1_Implied:
                    _addressState.Set(AddressOperand.A, AddressOperand.None, AddressOperand.X, AddressOperation.None, true);
                    return (MicroCode.Addressing, OpCodeStatus.None);
                case OpCodeStatus.TAY_1_Implied:
                    _addressState.Set(AddressOperand.A, AddressOperand.None, AddressOperand.Y, AddressOperation.None, true);
                    return (MicroCode.Addressing, OpCodeStatus.None);
                case OpCodeStatus.TXA_1_Implied:
                    _addressState.Set(AddressOperand.X, AddressOperand.None, AddressOperand.A, AddressOperation.None, true);
                    return (MicroCode.Addressing, OpCodeStatus.None);
                case OpCodeStatus.TXS_1_Implied:
                    _addressState.Set(AddressOperand.X, AddressOperand.None, AddressOperand.S, AddressOperation.None, true);
                    return (MicroCode.Addressing, OpCodeStatus.None);

                case OpCodeStatus.INC_1_ZeroPage:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.Memory, AddressOperation.Inc, true);
                    return (MicroCode.ZeroPage_1, OpCodeStatus.None);
                case OpCodeStatus.INX_1_Implied:
                    _addressState.Set(AddressOperand.X, AddressOperand.None, AddressOperand.X, AddressOperation.Inc, true);
                    return (MicroCode.Addressing, OpCodeStatus.None);
                case OpCodeStatus.INY_1_Implied:
                    _addressState.Set(AddressOperand.Y, AddressOperand.None, AddressOperand.Y, AddressOperation.Inc, true);
                    return (MicroCode.Addressing, OpCodeStatus.None);

                case OpCodeStatus.DEX_1_Implied:
                    _addressState.Set(AddressOperand.X, AddressOperand.None, AddressOperand.X, AddressOperation.Dec, true);
                    return (MicroCode.Addressing, OpCodeStatus.None);

                case OpCodeStatus.LDA_1_Immediate:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.A, AddressOperation.None, true);
                    return (MicroCode.Immediate, OpCodeStatus.None);
                case OpCodeStatus.LDA_1_Absolute:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.A, AddressOperation.None, true);
                    return (MicroCode.Absolute_1, OpCodeStatus.None);
                case OpCodeStatus.LDA_1_AbsoluteX:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.A, AddressOperation.None, true);
                    return (MicroCode.AbsoluteX_1, OpCodeStatus.None);
                case OpCodeStatus.LDA_1_IndirectY:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.A, AddressOperation.None, true);
                    return (MicroCode.IndirectY_1, OpCodeStatus.None);
                case OpCodeStatus.LDA_1_ZeroPage:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.A, AddressOperation.None, true);
                    return (MicroCode.ZeroPage_1, OpCodeStatus.None);

                case OpCodeStatus.JSR_1_Absolute:
                    _addressState.ResultA = (byte)((Registers.PC + 1) >> 8);
                    return (MicroCode.Push, OpCodeStatus.JSR_2_Absolute);
                case OpCodeStatus.JSR_2_Absolute:
                    _addressState.ResultA = (byte)((Registers.PC + 1) & 0xFF);
                    return (MicroCode.Push, OpCodeStatus.JSR_3_Absolute);
                case OpCodeStatus.JSR_3_Absolute:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.A, AddressOperation.None, true);
                    return (MicroCode.Absolute_1, OpCodeStatus.None);

                case OpCodeStatus.BIT_1_Absolute:
                    _addressState.Set(AddressOperand.A, AddressOperand.Memory, AddressOperand.None, AddressOperation.BitTest, true);
                    return (MicroCode.Absolute_1, OpCodeStatus.None);

                case OpCodeStatus.BPL_1_Relative:
                    if (Status.N)
                    {
                        Registers.PC++;
                        return (MicroCode.Nop, OpCodeStatus.None);
                    }
                    else
                    {
                        return (MicroCode.Nop, OpCodeStatus.Relative_Jump);
                    }

                case OpCodeStatus.BNE_1_Relative:
                    if (Status.Z)
                    {
                        Registers.PC++;
                        return (MicroCode.Nop, OpCodeStatus.None);
                    }
                    else
                    {
                        return (MicroCode.Nop, OpCodeStatus.Relative_Jump);
                    }

                case OpCodeStatus.BEQ_1_Relative:
                    if (Status.Z)
                    {
                        return (MicroCode.Nop, OpCodeStatus.Relative_Jump);
                    }
                    else
                    {
                        Registers.PC++;
                        return (MicroCode.Nop, OpCodeStatus.None);
                    }

                case OpCodeStatus.CMP_1_ZeroPage:
                    _addressState.Set(AddressOperand.A, AddressOperand.Memory, AddressOperand.None, AddressOperation.Compare, true);
                    return (MicroCode.ZeroPage_1, OpCodeStatus.None);
                case OpCodeStatus.CMP_1_Absolute:
                    _addressState.Set(AddressOperand.A, AddressOperand.Memory, AddressOperand.None, AddressOperation.Compare, true);
                    return (MicroCode.Absolute_1, OpCodeStatus.None);

                case OpCodeStatus.CPX_1_Immediate:
                    _addressState.Set(AddressOperand.X, AddressOperand.Memory, AddressOperand.None, AddressOperation.Compare, true);
                    return (MicroCode.Immediate, OpCodeStatus.None);

                case OpCodeStatus.AND_1_Immediate:
                    _addressState.Set(AddressOperand.A, AddressOperand.Memory, AddressOperand.None, AddressOperation.Compare, true);
                    return (MicroCode.Immediate, OpCodeStatus.None);

                case OpCodeStatus.RTI_1_Implied:
                    return (MicroCode.Pop, OpCodeStatus.RTI_2_Implied);
                case OpCodeStatus.RTI_2_Implied:
                    Status.Value = _addressState.ResultA;
                    return (MicroCode.Pop, OpCodeStatus.RTI_3_Implied);
                case OpCodeStatus.RTI_3_Implied:
                    _addressState.MemoryAddress = _addressState.ResultA;
                    return (MicroCode.Pop, OpCodeStatus.RTI_4_Implied);
                case OpCodeStatus.RTI_4_Implied:
                    _addressState.MemoryAddress |= (ushort)(_addressState.ResultA << 8);
                    return (MicroCode.Nop, OpCodeStatus.RTI_5_Implied);
                case OpCodeStatus.RTI_5_Implied:
                    _addressState.Set(AddressOperand.None, AddressOperand.None, AddressOperand.PC, AddressOperation.None, false);
                    return (MicroCode.Addressing, OpCodeStatus.None);

                case OpCodeStatus.JMP_1_Absolute:
                    _addressState.Set(AddressOperand.Memory, AddressOperand.None, AddressOperand.PC, AddressOperation.None, false);
                    return (MicroCode.Absolute_1, OpCodeStatus.None);

                case OpCodeStatus.RTS_1_Implied:
                    return (MicroCode.Pop, OpCodeStatus.RTS_2_Implied);
                case OpCodeStatus.RTS_2_Implied:
                    _addressState.MemoryAddress = _addressState.ResultA;
                    return (MicroCode.Pop, OpCodeStatus.RTS_3_Implied);
                case OpCodeStatus.RTS_3_Implied:
                    _addressState.MemoryAddress |= (ushort)(_addressState.ResultA << 8);
                    return (MicroCode.Nop, OpCodeStatus.RTS_4_Implied);
                case OpCodeStatus.RTS_4_Implied:
                    _addressState.MemoryAddress++;
                    return (MicroCode.Nop, OpCodeStatus.RTS_5_Implied);
                case OpCodeStatus.RTS_5_Implied:
                    _addressState.Set(AddressOperand.None, AddressOperand.None, AddressOperand.PC, AddressOperation.None, false);
                    return (MicroCode.Addressing, OpCodeStatus.None);
                default:
                    throw new InvalidProgramException($"invalid op code status: 0x{code:X}.");
            }
        }
    }
}
