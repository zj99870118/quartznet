using System;
using System.Text;

namespace Quartz.Impl.Calendar
{
    /// <summary>
    /// This implementation of the Calendar excludes (or includes - see below) a
    /// specified time range each day. For example, you could use this calendar to
    /// exclude business hours (8AM - 5PM) every day. Each <see cref="DailyCalendar" />
    /// only allows a single time range to be specified, and that time range may not
    /// * cross daily boundaries (i.e. you cannot specify a time range from 8PM - 5AM).
    /// If the property <see cref="invertTimeRange" /> is <see langword="false" /> (default),
    /// the time range defines a range of times in which triggers are not allowed to
    /// * fire. If <see cref="invertTimeRange" /> is <see langword="true" />, the time range
    /// is inverted: that is, all times <i>outside</i> the defined time range
    /// are excluded.
    /// <p>
    /// Note when using <see cref="DailyCalendar" />, it behaves on the same principals
    /// as, for example, WeeklyCalendar defines a set of days that are
    /// excluded <i>every week</i>. Likewise, <see cref="DailyCalendar" /> defines a
    /// set of times that are excluded <i>every day</i>.
    /// </p>
    /// </summary>
    /// <author>Mike Funk</author>
    /// <author>Aaron Craven</author>
    public class DailyCalendar : BaseCalendar
    {
        private static readonly string invalidHourOfDay = "Invalid hour of day: ";
        private static readonly string invalidMinute = "Invalid minute: ";
        private static readonly string invalidSecond = "Invalid second: ";
        private static readonly string invalidMillis = "Invalid millis: ";
        private static readonly string invalidTimeRange = "Invalid time range: ";
        private static readonly string separator = " - ";
        private static readonly long oneMillis = 1;
        private static readonly char colon = ':';

        private readonly string name;
        private int rangeStartingHourOfDay;
        private int rangeStartingMinute;
        private int rangeStartingSecond;
        private int rangeStartingMillis;
        private int rangeEndingHourOfDay;
        private int rangeEndingMinute;
        private int rangeEndingSecond;
        private int rangeEndingMillis;

        private bool invertTimeRange = false;


        /// <summary>
        /// Create a <see cref="DailyCalendar" /> with a time range defined by the
        /// specified strings and no baseCalendar. 
        ///	<param name="rangeStartingTime" /> and <param name="rangeEndingTime" />
        /// must be in the format &quot;HH:MM[:SS[:mmm]]&quot; where:
        /// <ul>
        ///     <li>
        ///         HH is the hour of the specified time. The hour should be
        ///          specified using military (24-hour) time and must be in the range
        ///          0 to 23.
        ///     </li>
        ///     <li>
        ///         MM is the minute of the specified time and must be in the range
        ///         0 to 59.
        ///     </li>
        ///     <li>
        ///         SS is the second of the specified time and must be in the range
        ///         0 to 59.
        ///     </li>
        ///     <li>
        ///         mmm is the millisecond of the specified time and must be in the
        ///         range 0 to 999.
        ///     </li>
        ///     <li>items enclosed in brackets ('[', ']') are optional.</li>
        ///     <li>
        ///         The time range starting time must be before the time range ending
        ///         time. Note this means that a time range may not cross daily 
        ///         boundaries (10PM - 2AM)
        ///     </li>  
        /// </ul>
        /// </summary>
        /// <param name="name">The name for this calendar.</param>
        public DailyCalendar(string name,
                             string rangeStartingTime,
                             string rangeEndingTime)
        {
            this.name = name;
            SetTimeRange(rangeStartingTime, rangeEndingTime);
        }


        /// <summary>
        /// Create a <see cref="DailyCalendar" /> with a time range defined by the
        /// specified strings and the specified baseCalendar. 
        /// <param name="rangeStartingTime" /> and <param name="rangeEndingTime" />
        /// must be in the format &quot;HH:MM[:SS[:mmm]]&quot; where:
        /// <ul>
        ///     <li>
        ///         HH is the hour of the specified time. The hour should be
        ///         specified using military (24-hour) time and must be in the range
        ///         0 to 23.
        ///     </li>
        ///     <li>
        ///         MM is the minute of the specified time and must be in the range
        ///         0 to 59.
        ///     </li>
        ///      <li>
        ///         SS is the second of the specified time and must be in the range
        ///         0 to 59.
        ///     </li>
        ///     <li>
        ///         mmm is the millisecond of the specified time and must be in the
        ///         range 0 to 999.
        ///     </li>
        ///     <li>
        ///         items enclosed in brackets ('[', ']') are optional.
        ///     </li>
        ///     <li>
        ///         The time range starting time must be before the time range ending
        ///         time. Note this means that a time range may not cross daily 
        ///          boundaries (10PM - 2AM)
        ///     </li>  
        ///  </ul>
        /// </summary>
        /// <param name="name">the name for the <see cref="DailyCalendar" /></param>
        /// <param name="baseCalendar">
        /// The base calendar for this calendar instance see BaseCalendar for more 
        /// information on base calendar functionality.
        /// </param>
        public DailyCalendar(string name,
                             ICalendar baseCalendar,
                             string rangeStartingTime,
                             string rangeEndingTime) : base(baseCalendar)
        {
            this.name = name;
            SetTimeRange(rangeStartingTime, rangeEndingTime);
        }

        /// <summary>
        /// Create a <see cref="DailyCalendar" /> with a time range defined by the
        /// specified values and no baseCalendar. Values are subject to
        /// the following validations:
        /// <ul>
        ///     <li>
        ///         Hours must be in the range 0-23 and are expressed using military
        ///		    (24-hour) time.
        ///     </li>
        ///		<li>Minutes must be in the range 0-59</li>
        ///		<li>Seconds must be in the range 0-59</li>
        ///		<li>Milliseconds must be in the range 0-999</li>
        ///		<li>
        ///         The time range starting time must be before the time range ending
        ///		    time. Note this means that a time range may not cross daily 
        ///		    boundaries (10PM - 2AM)
        ///     </li>  
        /// </ul>
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="rangeStartingHourOfDay">The range starting hour of day.</param>
        /// <param name="rangeStartingMinute">The range starting minute.</param>
        /// <param name="rangeStartingSecond">The range starting second.</param>
        /// <param name="rangeStartingMillis">The range starting millis.</param>
        /// <param name="rangeEndingHourOfDay">The range ending hour of day.</param>
        /// <param name="rangeEndingMinute">The range ending minute.</param>
        /// <param name="rangeEndingSecond">The range ending second.</param>
        /// <param name="rangeEndingMillis">The range ending millis.</param>
        public DailyCalendar(string name,
                             int rangeStartingHourOfDay,
                             int rangeStartingMinute,
                             int rangeStartingSecond,
                             int rangeStartingMillis,
                             int rangeEndingHourOfDay,
                             int rangeEndingMinute,
                             int rangeEndingSecond,
                             int rangeEndingMillis)
        {
            this.name = name;
            SetTimeRange(rangeStartingHourOfDay,
                         rangeStartingMinute,
                         rangeStartingSecond,
                         rangeStartingMillis,
                         rangeEndingHourOfDay,
                         rangeEndingMinute,
                         rangeEndingSecond,
                         rangeEndingMillis);
        }

        /// <summary>
        /// Create a <see cref="DailyCalendar" /> with a time range defined by the
        /// specified values and the specified <param name="baseCalendar" />. Values are
        /// subject to the following validations:
        ///	<ul>
        ///     <li>
        ///         Hours must be in the range 0-23 and are expressed using military
        ///		    (24-hour) time.
        ///     </li>
        ///		<li>Minutes must be in the range 0-59</li>
        ///		<li>Seconds must be in the range 0-59</li>
        ///		<li>Milliseconds must be in the range 0-999</li>
        ///		<li>
        ///         The time range starting time must be before the time range ending
        ///		    time. Note this means that a time range may not cross daily
        ///		    boundaries (10PM - 2AM)
        ///     </li>  
        ///	</ul> 
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="rangeStartingHourOfDay">The range starting hour of day.</param>
        /// <param name="rangeStartingMinute">The range starting minute.</param>
        /// <param name="rangeStartingSecond">The range starting second.</param>
        /// <param name="rangeStartingMillis">The range starting millis.</param>
        /// <param name="rangeEndingHourOfDay">The range ending hour of day.</param>
        /// <param name="rangeEndingMinute">The range ending minute.</param>
        /// <param name="rangeEndingSecond">The range ending second.</param>
        /// <param name="rangeEndingMillis">The range ending millis.</param>
        public DailyCalendar(string name,
                             ICalendar baseCalendar,
                             int rangeStartingHourOfDay,
                             int rangeStartingMinute,
                             int rangeStartingSecond,
                             int rangeStartingMillis,
                             int rangeEndingHourOfDay,
                             int rangeEndingMinute,
                             int rangeEndingSecond,
                             int rangeEndingMillis) : base(baseCalendar)
        {
            this.name = name;
            SetTimeRange(rangeStartingHourOfDay,
                         rangeStartingMinute,
                         rangeStartingSecond,
                         rangeStartingMillis,
                         rangeEndingHourOfDay,
                         rangeEndingMinute,
                         rangeEndingSecond,
                         rangeEndingMillis);
        }


        /// <summary>
        /// Create a <see cref="DailyCalendar" /> with a time range defined by the
        ///	specified <see cref="DateTime" />s and no 
        ///	baseCalendar. The Calendars are subject to the following
        ///	considerations:
        ///	<ul>
        ///     <li>
        ///         Only the time-of-day fields of the specified Calendars will be
        ///		    used (the date fields will be ignored)
        ///     </li>
        ///		<li>
        ///         The starting time must be before the ending time of the defined
        ///		    time range. Note this means that a time range may not cross
        ///		    daily boundaries (10PM - 2AM). <i>(because only time fields are
        ///		    are used, it is possible for two Calendars to represent a valid
        ///		    time range and 
        ///		    <c>rangeStartingCalendar.after(rangeEndingCalendar) ==  true</c>)
        ///			</i>
        ///     </li>  
        /// </ul> 
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="rangeStartingCalendar">The range starting calendar.</param>
        /// <param name="rangeEndingCalendar">The range ending calendar.</param>
        public DailyCalendar(string name,
                             DateTime rangeStartingCalendar,
                             DateTime rangeEndingCalendar)
        {
            this.name = name;
            SetTimeRange(rangeStartingCalendar, rangeEndingCalendar);
        }

        /// <summary>
        /// Create a <see cref="DailyCalendar" /> with a time range defined by the
        ///	specified <see cref="DateTime" />s and the specified 
        ///	<param name="baseCalendar" />. The Calendars are subject to the following
        ///	considerations:
        ///	<ul>
        ///     <li>
        ///         Only the time-of-day fields of the specified Calendars will be
        ///		    used (the date fields will be ignored)
        ///     </li>
        ///		<li>
        ///         The starting time must be before the ending time of the defined
        ///		    time range. Note this means that a time range may not cross
        ///		    daily boundaries (10PM - 2AM). <i>(because only time fields are
        ///		    are used, it is possible for two Calendars to represent a valid
        ///		    time range and 
        ///		    <c>rangeStartingCalendar.after(rangeEndingCalendar) == true</c>)</i>
        ///     </li>  
        /// </ul> 
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="rangeStartingCalendar">The range starting calendar.</param>
        /// <param name="rangeEndingCalendar">The range ending calendar.</param>
        public DailyCalendar(string name,
                             ICalendar baseCalendar,
                             DateTime rangeStartingCalendar,
                             DateTime rangeEndingCalendar) : base(baseCalendar)
        {
            this.name = name;
            SetTimeRange(rangeStartingCalendar, rangeEndingCalendar);
        }

        /// <summary>
        /// Create a <see cref="DailyCalendar" /> with a time range defined by the
        /// specified values and no baseCalendar. The values are 
        ///	subject to the following considerations:
        ///	<ul>
        ///     <li>
        ///         Only the time-of-day portion of the specified values will be
        ///		    used
        ///     </li>
        ///		<li>
        ///         The starting time must be before the ending time of the defined
        ///		    time range. Note this means that a time range may not cross
        ///		    daily boundaries (10PM - 2AM). <i>(because only time value are
        ///		    are used, it is possible for the two values to represent a valid
        ///		    time range and <c>rangeStartingTime &gt; rangeEndingTime</c>)</i>
        ///     </li>  
        /// </ul> 
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="rangeStartingTimeInMillis">The range starting time in millis.</param>
        /// <param name="rangeEndingTimeInMillis">The range ending time in millis.</param>
        public DailyCalendar(string name,
                             long rangeStartingTimeInMillis,
                             long rangeEndingTimeInMillis)
        {
            this.name = name;
            SetTimeRange(rangeStartingTimeInMillis,
                         rangeEndingTimeInMillis);
        }


        /// <summary>
        /// Create a <see cref="DailyCalendar" /> with a time range defined by the
        /// specified values and the specified <param name="baseCalendar" />. The values
        /// are subject to the following considerations:
        /// <ul>
        ///     <li>
        ///         Only the time-of-day portion of the specified values will be
        ///         used
        ///     </li>
        /// 	<li>
        ///         The starting time must be before the ending time of the defined
        ///         time range. Note this means that a time range may not cross
        ///         daily boundaries (10PM - 2AM). <i>(because only time value are
        ///         are used, it is possible for the two values to represent a valid
        ///         time range and <c>rangeStartingTime &gt; rangeEndingTime</c>)</i>
        ///     </li>
        /// </ul>
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="rangeStartingTimeInMillis">The range starting time in millis.</param>
        /// <param name="rangeEndingTimeInMillis">The range ending time in millis.</param>
        public DailyCalendar(string name,
                             ICalendar baseCalendar,
                             long rangeStartingTimeInMillis,
                             long rangeEndingTimeInMillis) : base(baseCalendar)
        {
            this.name = name;
            SetTimeRange(rangeStartingTimeInMillis,
                         rangeEndingTimeInMillis);
        }


        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return name; }
        }


        /// <summary>
        /// Determine whether the given time  is 'included' by the
        /// Calendar.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public override bool IsTimeIncluded(DateTime time)
        {
            if ((GetBaseCalendar() != null) &&
                (GetBaseCalendar().IsTimeIncluded(time) == false))
            {
                return false;
            }

            DateTime startOfDayInMillis = GetStartOfDay(time);
            DateTime endOfDayInMillis = GetEndOfDay(time);
            DateTime timeRangeStartingTimeInMillis =
                GetTimeRangeStartingTime(time);
            DateTime timeRangeEndingTimeInMillis =
                GetTimeRangeEndingTime(time);
            if (!invertTimeRange)
            {
                if ((time > startOfDayInMillis &&
                     time < timeRangeStartingTimeInMillis) ||
                    (time > timeRangeEndingTimeInMillis &&
                     time < endOfDayInMillis))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if ((time >= timeRangeStartingTimeInMillis) &&
                    (time <= timeRangeEndingTimeInMillis))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }


        /// <summary>
        /// Determine the next time (in milliseconds) that is 'included' by the
        /// Calendar after the given time. Return the original value if timeStamp is
        /// included. Return 0 if all days are excluded.
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        /// <seealso cref="ICalendar.GetNextIncludedTime"/>
        public override DateTime GetNextIncludedTime(DateTime time)
        {
            DateTime nextIncludedTime = time.AddMilliseconds(oneMillis);

            while (!IsTimeIncluded(nextIncludedTime))
            {
                if (!invertTimeRange)
                {
                    //If the time is in a range excluded by this calendar, we can
                    // move to the end of the excluded time range and continue 
                    // testing from there. Otherwise, if nextIncludedTime is 
                    // excluded by the baseCalendar, ask it the next time it 
                    // includes and begin testing from there. Failing this, add one
                    // millisecond and continue testing.
                    if ((nextIncludedTime >=
                         GetTimeRangeStartingTime(nextIncludedTime)) &&
                        (nextIncludedTime <=
                         GetTimeRangeEndingTime(nextIncludedTime)))
                    {
                        nextIncludedTime =
                            GetTimeRangeEndingTime(nextIncludedTime).AddMilliseconds(oneMillis);
                    }
                    else if ((GetBaseCalendar() != null) &&
                             (!GetBaseCalendar().IsTimeIncluded(nextIncludedTime)))
                    {
                        nextIncludedTime =
                            GetBaseCalendar().GetNextIncludedTime(nextIncludedTime);
                    }
                    else
                    {
                        nextIncludedTime = nextIncludedTime.AddMilliseconds(1);
                    }
                }
                else
                {
                    //If the time is in a range excluded by this calendar, we can
                    // move to the end of the excluded time range and continue 
                    // testing from there. Otherwise, if nextIncludedTime is 
                    // excluded by the baseCalendar, ask it the next time it 
                    // includes and begin testing from there. Failing this, add one
                    // millisecond and continue testing.
                    if (nextIncludedTime <
                        GetTimeRangeStartingTime(nextIncludedTime))
                    {
                        nextIncludedTime =
                            GetTimeRangeStartingTime(nextIncludedTime);
                    }
                    else if (nextIncludedTime >
                             GetTimeRangeEndingTime(nextIncludedTime))
                    {
                        //(move to start of next day)
                        nextIncludedTime = GetEndOfDay(nextIncludedTime);
                        nextIncludedTime = nextIncludedTime.AddMilliseconds(1);
                    }
                    else if ((GetBaseCalendar() != null) &&
                             (!GetBaseCalendar().IsTimeIncluded(nextIncludedTime)))
                    {
                        nextIncludedTime =
                            GetBaseCalendar().GetNextIncludedTime(nextIncludedTime);
                    }
                    else
                    {
                        nextIncludedTime = nextIncludedTime.AddMilliseconds(1);
                    }
                }
            }

            return nextIncludedTime;
        }


        /// <summary>
        /// Returns the start time of the time range of the day 
        /// specified in <param name="time" />.
        /// </summary>
        /// <returns>
        ///     a DateTime representing the start time of the
        ///     time range for the specified date.
        /// </returns>
        public DateTime GetTimeRangeStartingTime(DateTime time)
        {
            DateTime rangeStartingTime = new DateTime(time.Year, time.Month, time.Day,
                                                      rangeStartingHourOfDay, rangeStartingMinute,
                                                      rangeStartingSecond, rangeStartingMillis);
            return rangeStartingTime;
        }

        /// <summary>
        /// Returns the end time of the time range of the day
        /// specified in <param name="time" />
        /// </summary>
        /// <returns>
        /// A DateTime representing the end time of the
        /// time range for the specified date.
        /// </returns>
        public DateTime GetTimeRangeEndingTime(DateTime time)
        {
            DateTime rangeEndingTime = new DateTime(time.Year, time.Month, time.Day,
                                                    rangeEndingHourOfDay, rangeEndingMinute,
                                                    rangeEndingSecond, rangeEndingMillis);
            return rangeEndingTime;
        }

        /// <summary>
        /// Indicates whether the time range represents an inverted time range (see
        /// class description).
        /// </summary>
        /// <value><c>true</c> if invert time range; otherwise, <c>false</c>.</value>
        public bool InvertTimeRange
        {
            get { return invertTimeRange; }
            set { invertTimeRange = value; }
        }


        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            StringBuilder buffer = new StringBuilder();
            buffer.Append(Name);
            buffer.Append(": base calendar: [");
            if (GetBaseCalendar() != null)
            {
                buffer.Append(GetBaseCalendar().ToString());
            }
            else
            {
                buffer.Append("null");
            }

            const string THREE_DIGIT_FORMAT = "000";
            const string TWO_DIGIT_FORMAT = "00";

            buffer.Append("], time range: '");
            buffer.Append(rangeStartingHourOfDay.ToString(TWO_DIGIT_FORMAT));
            buffer.Append(":");
            buffer.Append(rangeStartingMinute.ToString(TWO_DIGIT_FORMAT));
            buffer.Append(":");
            buffer.Append(rangeStartingSecond.ToString(TWO_DIGIT_FORMAT));
            buffer.Append(":");
            buffer.Append(rangeStartingMillis.ToString(THREE_DIGIT_FORMAT));
            buffer.Append(" - ");
            buffer.Append(rangeEndingHourOfDay.ToString(TWO_DIGIT_FORMAT));
            buffer.Append(":");
            buffer.Append(rangeEndingMinute.ToString(TWO_DIGIT_FORMAT));
            buffer.Append(":");
            buffer.Append(rangeEndingSecond.ToString(TWO_DIGIT_FORMAT));
            buffer.Append(":");
            buffer.Append(rangeEndingMillis.ToString(THREE_DIGIT_FORMAT));
            buffer.AppendFormat("', inverted: {0}]", invertTimeRange);
            return buffer.ToString();
        }


        /// <summary>
        /// Sets the time range for the <see cref="DailyCalendar" /> to the times 
        /// represented in the specified Strings. 
        /// </summary>
        /// <param name="rangeStartingTimeString">The range starting time string.</param>
        /// <param name="rangeEndingTimeString">The range ending time string.</param>
        private void SetTimeRange(string rangeStartingTimeString,
                                  string rangeEndingTimeString)
        {
            string[] rangeStartingTime;
            int rangeStartingHourOfDay;
            int rangeStartingMinute;
            int rangeStartingSecond;
            int rangeStartingMillis;

            string[] rangeEndingTime;
            int rangeEndingHourOfDay;
            int rangeEndingMinute;
            int rangeEndingSecond;
            int rangeEndingMillis;

            rangeStartingTime = rangeStartingTimeString.Split(colon);

            if ((rangeStartingTime.Length < 2) || (rangeStartingTime.Length > 4))
            {
                throw new ArgumentException(string.Format("Invalid time string '{0}'", rangeStartingTimeString));
            }

            rangeStartingHourOfDay = Convert.ToInt32(rangeStartingTime[0]);
            rangeStartingMinute = Convert.ToInt32(rangeStartingTime[1]);
            if (rangeStartingTime.Length > 2)
            {
                rangeStartingSecond = Convert.ToInt32(rangeStartingTime[2]);
            }
            else
            {
                rangeStartingSecond = 0;
            }
            if (rangeStartingTime.Length == 4)
            {
                rangeStartingMillis = Convert.ToInt32(rangeStartingTime[3]);
            }
            else
            {
                rangeStartingMillis = 0;
            }

            rangeEndingTime = rangeEndingTimeString.Split(colon);

            if ((rangeEndingTime.Length < 2) || (rangeEndingTime.Length > 4))
            {
                throw new ArgumentException(string.Format("Invalid time string '{0}'", rangeEndingTimeString));
            }

            rangeEndingHourOfDay = Convert.ToInt32(rangeEndingTime[0]);
            rangeEndingMinute = Convert.ToInt32(rangeEndingTime[1]);
            if (rangeEndingTime.Length > 2)
            {
                rangeEndingSecond = Convert.ToInt32(rangeEndingTime[2]);
            }
            else
            {
                rangeEndingSecond = 0;
            }
            if (rangeEndingTime.Length == 4)
            {
                rangeEndingMillis = Convert.ToInt32(rangeEndingTime[3]);
            }
            else
            {
                rangeEndingMillis = 0;
            }

            SetTimeRange(rangeStartingHourOfDay,
                         rangeStartingMinute,
                         rangeStartingSecond,
                         rangeStartingMillis,
                         rangeEndingHourOfDay,
                         rangeEndingMinute,
                         rangeEndingSecond,
                         rangeEndingMillis);
        }


        /// <summary>
        /// Sets the time range for the <see cref="DailyCalendar" /> to the times
        /// represented in the specified values. 
        /// </summary>
        /// <param name="rangeStartingHourOfDay">The range starting hour of day.</param>
        /// <param name="rangeStartingMinute">The range starting minute.</param>
        /// <param name="rangeStartingSecond">The range starting second.</param>
        /// <param name="rangeStartingMillis">The range starting millis.</param>
        /// <param name="rangeEndingHourOfDay">The range ending hour of day.</param>
        /// <param name="rangeEndingMinute">The range ending minute.</param>
        /// <param name="rangeEndingSecond">The range ending second.</param>
        /// <param name="rangeEndingMillis">The range ending millis.</param>
        private void SetTimeRange(int rangeStartingHourOfDay,
                                  int rangeStartingMinute,
                                  int rangeStartingSecond,
                                  int rangeStartingMillis,
                                  int rangeEndingHourOfDay,
                                  int rangeEndingMinute,
                                  int rangeEndingSecond,
                                  int rangeEndingMillis)
        {
            Validate(rangeStartingHourOfDay,
                     rangeStartingMinute,
                     rangeStartingSecond,
                     rangeStartingMillis);

            Validate(rangeEndingHourOfDay,
                     rangeEndingMinute,
                     rangeEndingSecond,
                     rangeEndingMillis);

            DateTime startCal = DateTime.Now;
            startCal =
                new DateTime(startCal.Year, startCal.Month, startCal.Day, rangeStartingHourOfDay, rangeStartingMinute,
                             rangeStartingSecond, rangeStartingMillis);

            DateTime endCal = DateTime.Now;
            endCal =
                new DateTime(endCal.Year, endCal.Month, endCal.Day, rangeEndingHourOfDay, rangeEndingMinute,
                             rangeEndingSecond, rangeEndingMillis);


            if (! (startCal < endCal))
            {
                throw new ArgumentException(string.Format("{0}{1}:{2}:{3}:{4}{5}{6}:{7}:{8}:{9}",
                                                          invalidTimeRange, rangeStartingHourOfDay, rangeStartingMinute,
                                                          rangeStartingSecond, rangeStartingMillis, separator,
                                                          rangeEndingHourOfDay, rangeEndingMinute, rangeEndingSecond,
                                                          rangeEndingMillis));
            }

            this.rangeStartingHourOfDay = rangeStartingHourOfDay;
            this.rangeStartingMinute = rangeStartingMinute;
            this.rangeStartingSecond = rangeStartingSecond;
            this.rangeStartingMillis = rangeStartingMillis;
            this.rangeEndingHourOfDay = rangeEndingHourOfDay;
            this.rangeEndingMinute = rangeEndingMinute;
            this.rangeEndingSecond = rangeEndingSecond;
            this.rangeEndingMillis = rangeEndingMillis;
        }

        /// <summary>
        /// Sets the time range for the <see cref="DailyCalendar" /> to the times
        /// represented in the specified <see cref="DateTime" />s. 
        /// </summary>
        /// <param name="rangeStartingCalendar">The range starting calendar.</param>
        /// <param name="rangeEndingCalendar">The range ending calendar.</param>
        private void SetTimeRange(DateTime rangeStartingCalendar,
                                  DateTime rangeEndingCalendar)
        {
            SetTimeRange(
                rangeStartingCalendar.Hour,
                rangeStartingCalendar.Minute,
                rangeStartingCalendar.Second,
                rangeStartingCalendar.Millisecond,
                rangeEndingCalendar.Hour,
                rangeEndingCalendar.Minute,
                rangeEndingCalendar.Second,
                rangeEndingCalendar.Millisecond);
        }

        /// <summary>
        /// Sets the time range for the <see cref="DailyCalendar" /> to the times
        /// represented in the specified values. 
        /// </summary>
        /// <param name="rangeStartingTime">The range starting time.</param>
        /// <param name="rangeEndingTime">The range ending time.</param>
        private void SetTimeRange(long rangeStartingTime,
                                  long rangeEndingTime)
        {
            SetTimeRange(new DateTime(rangeStartingTime), new DateTime(rangeEndingTime));
        }


        /// <summary>
        /// Gets the start of day, practically zeroes time part.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        private static DateTime GetStartOfDay(DateTime time)
        {
            return time.Date;
        }

        /// <summary>
        /// Gets the end of day, pratically sets time parts to maximum allowed values.
        /// </summary>
        /// <param name="time">The time.</param>
        /// <returns></returns>
        private static DateTime GetEndOfDay(DateTime time)
        {
            DateTime endOfDay = new DateTime(time.Year, time.Month, time.Day, 23, 59, 59, 999);
            return endOfDay;
        }


        /// <summary>
        /// Checks the specified values for validity as a set of time values.
        /// </summary>
        /// <param name="hourOfDay">The hour of day.</param>
        /// <param name="minute">The minute.</param>
        /// <param name="second">The second.</param>
        /// <param name="millis">The millis.</param>
        private static void Validate(int hourOfDay, int minute, int second, int millis)
        {
            if (hourOfDay < 0 || hourOfDay > 23)
            {
                throw new ArgumentException(invalidHourOfDay + hourOfDay);
            }
            if (minute < 0 || minute > 59)
            {
                throw new ArgumentException(invalidMinute + minute);
            }
            if (second < 0 || second > 59)
            {
                throw new ArgumentException(invalidSecond + second);
            }
            if (millis < 0 || millis > 999)
            {
                throw new ArgumentException(invalidMillis + millis);
            }
        }
    }
}