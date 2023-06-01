/*

Reading ECG signal using Picoscope

Instruction.
connect output of differential amplifier to chA.
The Picoscope 2000 is only equipment we tested.


 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PS2000Imports;

using MDataplot;

namespace Emotion_beat
{
    class PicoSetting
    {
        public double SampleFreqHz = 1000;  //Hz
        public double RecordTime = 1.0;     //Sec   determine the _appbuffersize

        //public Boolean Trig = false;
        public Imports.Channel TrigSource = Imports.Channel.ChannelA;
        
        public Imports.ThresholdDirection TrigDirection = Imports.ThresholdDirection.Rising;
        public short TrigLevelmv = 0;
        public int TrigDelay = 0;
        public uint noOfSamplesPerAggregate = 1;
        public ChannelSettings[] channelSettings; //was private
        private int _channelCount = PS2000_Const.DUAL_SCOPE;
        public PicoSetting()
        {
            channelSettings = new ChannelSettings[2];  //Dual channel
            for (int i = 0; i < 2; i++)
            {
                channelSettings[i].enabled = 1;
                channelSettings[i].DCcoupled = 1; //DC coupled
                channelSettings[i].range = Imports.Range.Range_5V;
            }
        }
    }

    //Enums for limited function of PS2205A
    class PS2000_Const
    {
        public const int BUFFER_SIZE = 8064; //mysterious number... works with 2250A
        public const int DUAL_SCOPE = 2;

        public enum TrigChannel : short
        {
            ChannelA = 0,
            ChannelB = 1,
            None     = 5
        }

        public enum TrigDirection :int
        {
            Above,
            Below,
            Rising,
            Falling,
            RisingOrFalling
        }

        public enum RangeCode :short
        {
            Range_20mV = 1,
            Range_50mV = 2,
            Range_100mV = 3,
            Range_200mV = 4,
            Range_500mV = 5,
            Range_1V = 6,
            Range_2V = 7,
            Range_5V = 8,
            Range_10V = 9,
            Range_20V = 10
        }

        public enum Rangemv : int
        {
            Range_20mV = 20,
            Range_50mV = 50,
            Range_100mV = 100,
            Range_200mV = 200,
            Range_500mV = 500,
            Range_1V = 1000,
            Range_2V = 2000,
            Range_5V = 5000,
            Range_10V = 10000,
            Range_20V = 20000
        }

        public enum TimeUnit
        {
            us,
            ms,
            sec
        }

        public enum FreqUnit
        {
            mHz,
            Hz,
            kHz,
            MHz
        }

    }

    struct ChannelSettings
    {
        public short DCcoupled;
        public Imports.Range range;
        public short enabled;
    }

    class PicoECG
    {
        public short _handle;

        //setting data.
        public PicoSetting setting;

        //Streaming (reading index, settings, etc)
        Boolean _streaming = false;
        uint _totalSampleCount = 0;
        uint _previousSamples = 0; // Keep track of the previous total number of samples
        bool _Triggered = false;
        uint _TriggeredAt = 0;

        bool _autoStop;
        bool _appBufferFull;
        public short[][] _appBuffer = new short[PS2000_Const.DUAL_SCOPE][]; //used in streaming mode. (Call back)

        private uint _OverViewBufferSize = 150000;
        private uint _MaxSamples = 10800000;
        
        private int _channelCount = PS2000_Const.DUAL_SCOPE;

        public double CurrentTimeStamp
        {
            get
            {
                if (_streaming)
                {
                    if ((_Triggered && setting.TrigSource != Imports.Channel.None) || setting.TrigSource == Imports.Channel.None) //after triggering.
                        return (_totalSampleCount - _TriggeredAt) / setting.SampleFreqHz * setting.noOfSamplesPerAggregate;
                    else
                        return 0;
                }
                else
                {
                    return 0;
                }
            }
        }
        
        public void Close()
        {
            Imports.CloseUnit(_handle);
        }

        public bool InitPico()
        {
            //OPEN Device
            if ((_handle = Imports.OpenUnit()) <= 0)
            {
                Form1._Form1.LogAppend(string.Format("Unable to open device"));
                Form1._Form1.LogAppend(string.Format("Error code : {0}", _handle));
                return false;
            }
            else //successfully opened.
            {
                setting = new PicoSetting(); // Create-Initialize setting class.
                Form1._Form1.LogAppend("Picoscope Opened successfully\r\n");
                return true;
            }
        }

        /****************************************************************************
         * StreamingCallback
         * used by data streaming collection calls, on receipt of data.
         * used to set global flags etc checked by user routines
         ****************************************************************************/
        unsafe void StreamingCallback(short** overviewBuffers,
                                          short overFlow,
                                          uint triggeredAt,
                                          short triggered,
                                          short auto_stop,
                                          uint nValues)
        {
            // used for streaming
            _autoStop = auto_stop != 0;

            //if Trigger is detected. Save it for other inqury for data.
            if (triggered>0)
            {
                _Triggered = true;
                _TriggeredAt = triggeredAt + _totalSampleCount;
                Form1._Form1.LogAppend(string.Format("[[TRIG at {0}]\r\n",_TriggeredAt));
            }

            //nValue --- The number of sample stored in overview buffers (sample datas) number of values.

            if (nValues > 0 && !_appBufferFull)
            {
                try
                {
                    for (int i = (int)_totalSampleCount; i < nValues + _totalSampleCount; i++)
                    {
                        for (int channel = 0; channel < _channelCount; channel++)
                        {
                            if (Convert.ToBoolean(setting.channelSettings[channel].enabled))
                            {
                                _appBuffer[channel][i] = overviewBuffers[channel * 2][i - _totalSampleCount]; //Only copying max data from buffers
                                //in case of using agregation, max represent the peak of lost points between samples. https://www.picotech.com/library/oscilloscopes/streaming-mode
                            }
                        }
                    }
                }
                catch (Exception) // If trying to place data 
                {
                    _appBufferFull = true;
                    Form1._Form1.LogAppend(string.Format("Appbuffer full collection cancelled"));
                }
            }

            _totalSampleCount += nValues;
        }


        public void StartStreaming()
        {
            short autoStop = 1;
            uint appBufferSize = (uint)(setting.SampleFreqHz * setting.RecordTime * 1.05); //5% of safety factor..

            _streaming = true;

            //Reset read pointers
            _totalSampleCount = 0;
            _previousSamples = 0;
            _autoStop = false; //this flag can be changed in Call back.
            _appBufferFull = false;
            _Triggered = false;
            _TriggeredAt = 0;

            for (int i = 0; i < _channelCount; i++)
            {
                if (Convert.ToBoolean(setting.channelSettings[i].enabled))
                {
                    _appBuffer[i] = new short[appBufferSize]; // Set local buffer to hold all values
                }
            }

            //configure channel setting.
            for (int i = 0; i < _channelCount; i++)
            {
                Imports.SetChannel(_handle,
                                    (Imports.Channel)(i),
                                    setting.channelSettings[i].enabled,
                                    setting.channelSettings[i].DCcoupled,
                                    setting.channelSettings[i].range);
            }

            //Trigger setting.
            SetTrigger();

            //For now, the unit is fixed to microsecond.
            uint sampleinterval = (uint)(1.0e6 / setting.SampleFreqHz);
            Imports.ReportedTimeUnits unit = Imports.ReportedTimeUnits.MicroSeconds;

            //Start streaming
            Imports.ps2000_run_streaming_ns(_handle, sampleinterval, unit, _MaxSamples, autoStop,
                           setting.noOfSamplesPerAggregate, _OverViewBufferSize);

            Form1._Form1.LogAppend("Streaming Started.\r\n");
        }

        public void StopStreaming()
        {
            Imports.Stop(_handle);
            _streaming = false;
            Form1._Form1.LogAppend("Streaming stopped.\r\n");
        }
        
        //for repeated streaming.
        public void resetstreaming()
        {   
            //stop
            Imports.Stop(_handle);

            //Reset read pointers
            _totalSampleCount = 0;
            _previousSamples = 0;
            _Triggered = false;
            _TriggeredAt = 0;

            //it seems SetTrigger is not necessary.
            //SetTrigger();

            //For now, the unit is fixed to microsecond.
            uint sampleinterval = (uint)(1.0e6 / setting.SampleFreqHz);
            Imports.ReportedTimeUnits unit = Imports.ReportedTimeUnits.MicroSeconds;

            //Start streaming
            Imports.ps2000_run_streaming_ns(_handle, sampleinterval, unit, _MaxSamples, 1,
                           setting.noOfSamplesPerAggregate, _OverViewBufferSize);

        }

        public unsafe WAVE[] ReveiveAvailableData()
        {
            short previousBufferOverrun = 0;
            uint sampleCount = 0;
            uint appBufferSize = (uint)(setting.SampleFreqHz * setting.RecordTime); //5% of safety factor..

            //seek data
            Imports.ps2000_get_streaming_last_values(_handle, StreamingCallback);

            short status = Imports.OverviewBufferStatus(_handle, out previousBufferOverrun);
            if (previousBufferOverrun > 0)
            {
                Form1._Form1.LogAppend(string.Format("Overview buffer overrun detected.\r\n"));
            }

            if (_previousSamples != _totalSampleCount)
            {
                //Check if datas in buffer contain triggered point.
                //if (_Triggered)
                //{
                //    Form1._Form1.LogAppend(string.Format("My trigger at {0}\r\n", _TriggeredAt));
                //}
                //Form1._Form1.LogAppend(string.Format("Collected {0,4} samples, Total = {1,5}", sampleCount, _totalSampleCount));

                WAVE[] tempwave = new MDataplot.WAVE[_channelCount];

                if (_totalSampleCount < appBufferSize)
                {
                    for(short ch_index=0;ch_index <_channelCount; ch_index++)
                    {
                        //create wave regardless the channel is enabled (all invalid value)
                        tempwave[ch_index] = new WAVE((int)(_totalSampleCount - _previousSamples));
                        if (setting.channelSettings[ch_index].enabled > 0)
                        {
                            for (int i = (int)_previousSamples; i < _totalSampleCount; i++)
                            {
                                if (setting.TrigSource != Imports.Channel.None && _Triggered && i >= _TriggeredAt)
                                {
                                    double timestamp = (i - _TriggeredAt) / setting.SampleFreqHz * setting.noOfSamplesPerAggregate;
                                    tempwave[ch_index].AddPoint(timestamp, adc_to_mv(_appBuffer[ch_index][i], ch_index));
                                }
                                else if (setting.TrigSource == Imports.Channel.None)
                                {
                                    double timestamp = i / setting.SampleFreqHz * setting.noOfSamplesPerAggregate;
                                    tempwave[ch_index].AddPoint(timestamp, adc_to_mv(_appBuffer[ch_index][i], ch_index));
                                }
                            }
                        }
                    }
                    
                }
                _previousSamples = _totalSampleCount;
                return (tempwave);

            }
            else
            {
                WAVE[] tempwave = new WAVE[_channelCount];
                tempwave[0] = new WAVE(0);
                tempwave[1] = new WAVE(0);
                return (tempwave);
            }
        }

        public void SetTrigger()
        {
            
            Imports.TriggerChannelProperties[] channelProperties = null;
            short nChannelProperties = 0;
            Imports.TriggerConditions[] triggerConditions = null;
            short nTriggerConditions = 0;
            Imports.ThresholdDirection[] directions = null;
            Pwq pwq = null;
            uint delay = 0;
            int autoTriggerMs = 0; //timeout

            short status = 0;

            if (setting.TrigSource == Imports.Channel.ChannelA)
            {
                short triggerVoltage = mv_to_adc(setting.TrigLevelmv, (short)setting.TrigSource); // ChannelInfo stores ADC counts channelA
                channelProperties = new Imports.TriggerChannelProperties[] {
                                             new Imports.TriggerChannelProperties(
                                             triggerVoltage,
                                             triggerVoltage,
                                             256*10,
                                             setting.TrigSource,
                                             Imports.ThresholdMode.Level)};

                triggerConditions = new Imports.TriggerConditions[] {
                                        new Imports.TriggerConditions(
                                            Imports.TriggerState.True,                              // Channel A
                                            Imports.TriggerState.DontCare,                          // Channel B
                                            Imports.TriggerState.DontCare,                          // Channel C
                                            Imports.TriggerState.DontCare,                          // Channel D
                                            Imports.TriggerState.DontCare,                          // external
                                            Imports.TriggerState.DontCare                           // pwq
                                            )};

                directions = new Imports.ThresholdDirection[] {
                                            setting.TrigDirection,                               // Channel A
                                            Imports.ThresholdDirection.None,                        // Channel B
                                            Imports.ThresholdDirection.None,                        // Channel C
                                            Imports.ThresholdDirection.None,                        // Channel D
                                            Imports.ThresholdDirection.None };                      // ext
                nChannelProperties = 1;
                nTriggerConditions = 1;
                pwq = null;
                autoTriggerMs = 1000; //timeout

            }
            else if (setting.TrigSource == Imports.Channel.ChannelB)
            {
                short triggerVoltage = mv_to_adc(setting.TrigLevelmv, (short)setting.TrigSource); // ChannelInfo stores ADC counts channelA
                channelProperties = new Imports.TriggerChannelProperties[] {
                                             new Imports.TriggerChannelProperties(triggerVoltage,
                                             triggerVoltage,
                                             256*10,
                                             setting.TrigSource,
                                             Imports.ThresholdMode.Level)};

                triggerConditions = new Imports.TriggerConditions[] {
                                        new Imports.TriggerConditions(
                                            Imports.TriggerState.DontCare,                          // Channel A
                                            Imports.TriggerState.True,                              // Channel B
                                            Imports.TriggerState.DontCare,                          // Channel C
                                            Imports.TriggerState.DontCare,                          // Channel D
                                            Imports.TriggerState.DontCare,                          // external
                                            Imports.TriggerState.DontCare                           // pwq
                                            )};

                directions = new Imports.ThresholdDirection[] {
                                            Imports.ThresholdDirection.None,                        // Channel A
                                            setting.TrigDirection,                                  // Channel B
                                            Imports.ThresholdDirection.None,                        // Channel C
                                            Imports.ThresholdDirection.None,                        // Channel D
                                            Imports.ThresholdDirection.None };                      // ext

                nChannelProperties = 1;
                nTriggerConditions = 1;
                pwq = null;
                autoTriggerMs = 1000; //timeout
            }
            // equivalent ps2000SetAdvTriggerChannelProperties
            status = Imports.SetTriggerChannelProperties(_handle, channelProperties, nChannelProperties, autoTriggerMs);

            //equivalent "ps2000SetAdvTriggerChannelConditions"
            status = Imports.SetTriggerChannelConditions(_handle, triggerConditions, nTriggerConditions);

            if (directions == null)
            {
                directions = new Imports.ThresholdDirection[] {
                                    Imports.ThresholdDirection.None,
                                    Imports.ThresholdDirection.None,
                                    Imports.ThresholdDirection.None,
                                    Imports.ThresholdDirection.None,
                                    Imports.ThresholdDirection.None,
                                    Imports.ThresholdDirection.None};
            }

            //equivalent to "ps2000SetAdvTriggerChannelDirections()"  see manual.
            status = Imports.SetTriggerChannelDirections(_handle,
                                                              directions[(int)Imports.Channel.ChannelA],
                                                              directions[(int)Imports.Channel.ChannelB],
                                                              directions[(int)Imports.Channel.ChannelC],
                                                              directions[(int)Imports.Channel.ChannelD],
                                                              directions[(int)Imports.Channel.External]);

            //fine tune is possible for post triggering delay. (sample count)
            //status = Imports.SetTriggerDelay(_handle, (uint)delay, 0);

            //pre triggering delay can be tune from -100% to 100%
            status = Imports.SetTriggerDelay(_handle, 0, setting.TrigDelay);

            if (pwq == null)
            {
                pwq = new Pwq(null, 0, Imports.ThresholdDirection.None, 0, 0, Imports.PulseWidthType.None);
            }

            status = Imports.SetPulseWidthQualifier(_handle, pwq.conditions,
                                                    pwq.nConditions, pwq.direction,
                                                    pwq.lower, pwq.upper, pwq.type);

        }

        /****************************************************************************
         * adc_to_mv
         * Convert an 16-bit ADC count into millivolts
         ****************************************************************************/
        int adc_to_mv(int raw, short ch)
        {
            int rangemv = (int)Enum.Parse(typeof(PS2000_Const.Rangemv), ((PS2000_Const.RangeCode)(setting.channelSettings[ch].range)).ToString());
            return (raw * rangemv) / Imports.PS2000_MAX_VALUE;
        }

        /****************************************************************************
         * mv_to_adc
         * Convert a millivolt value into a 16-bit ADC count
         *  (useful for setting trigger thresholds)
         ****************************************************************************/
        short mv_to_adc(short mv, short ch)
        {
            int rangemv = (int)Enum.Parse(typeof(PS2000_Const.Rangemv), ((PS2000_Const.RangeCode)(setting.channelSettings[ch].range)).ToString());
            return (short)((mv * Imports.PS2000_MAX_VALUE) / rangemv);
        }
    }


    //PWQ --- PulseWidthQualifier (pulse width triggering?)
    class Pwq
    {
        public Imports.PwqConditions[] conditions;
        public short nConditions;
        public Imports.ThresholdDirection direction;
        public uint lower;
        public uint upper;
        public Imports.PulseWidthType type;

        public Pwq(Imports.PwqConditions[] conditions,
            short nConditions,
            Imports.ThresholdDirection direction,
            uint lower, uint upper,
            Imports.PulseWidthType type)
        {
            this.conditions = conditions;
            this.nConditions = nConditions;
            this.direction = direction;
            this.lower = lower;
            this.upper = upper;
            this.type = type;
        }
    }
}
