$(document).ready(function () {
    var Select1;
    var Select2;

    if ($('.formItem #BeginDate').length) {
        var Select1 = $('.formItem #BeginDate');
        var Select2 = $('.formItem #EndDate');

        Select1.datepicker({
            dateFormat: 'dd.mm.yy',
            showAnim: 'slideDown',
            firstDay: 1,
            onSelect: function (dateText) {
                var euDateFormat = $(this).val(),
                newDate = new Date(euDateFormat.substring(6, 10), parseInt(euDateFormat.substring(3, 5)) - 1, euDateFormat.substring(0, 2)),
                    lastDay = new Date(newDate.getFullYear(), (newDate.getMonth()) + 1, 0),
                    nextDay = ((newDate.getDate() + 4) <= 9 ? "0" + (newDate.getDate() + 4) : (newDate.getDate() + 4)),
                    nextMonth = ((newDate.getMonth() +1) <= 9 ? "0" + (newDate.getMonth() + 1) : (newDate.getMonth() + 1)),
                    nextYear = newDate.getFullYear();
                    
                if ((newDate.getDate() + 4) > lastDay.getDate()) {
                    var differenz = lastDay.getDate() - newDate.getDate();
                    differenz = differenz >= 0 ? differenz : 0;
                    nextDay = ((newDate.getDate() + 4 - differenz) - lastDay.getDate()) <= 9 ? "0" + ((newDate.getDate() + 4) - lastDay.getDate()) : ((newDate.getDate() + 4) - lastDay.getDate());
                    nextMonth = ((newDate.getMonth() + 1) <= 9 ? ("0" + (newDate.getMonth() + 2)) : (newDate.getMonth() + 2));
                    if (nextMonth == 13) {
                        nextMonth =  '0' + 1;
                        nextYear = nextYear + 1;
                    }
                }
                                
                Select2.val(
                    nextDay
                    + "."
                    + nextMonth
                    + "."
                    + 
                    nextYear
                );
            },
            beforeShowDay: function (date) {
                if (date.getDay() == 1)
                    return [1];
                else
                    return [0];
            }
        });

        Select2.datepicker({
            dateFormat: 'dd.mm.yy',
            showAnim: 'slideDown',
            firstDay: 1,
            onSelect: function (dateText) {
                var euDateFormat = $(this).val(),
                newDate = new Date(euDateFormat.substring(6, 10), (parseInt(euDateFormat.substring(3, 5)) - 1), euDateFormat.substring(0, 2)),
                    firstDay = new Date(newDate.getFullYear(), (newDate.getMonth()) + 1, 1),
                    lastDayMonthBefore = firstDay = new Date(newDate.getFullYear(), (newDate.getMonth()), 0),
                    nextDay = ((newDate.getDate() + 4) <= 9 ? "0" + (newDate.getDate() + 4) : (newDate.getDate() - 4)),
                    nextMonth = ((newDate.getMonth()) <= 9 ? "0" + (newDate.getMonth()) : (newDate.getMonth())),
                    nextYear = newDate.getFullYear();

                if ((newDate.getDate() - 4) < firstDay.getDate()) {
                    var differenz = firstDay.getDate() + newDate.getDate();
                    differenz = differenz < 0 ? differenz : 0;
                    //alert(firstDay);
                    //alert(newDate);
                    //alert(lastDayMonthBefore);
                    nextDay = ((newDate.getDate() + 4 - differenz) - firstDay.getDate()) <= 9 ? "0" + ((newDate.getDate() + 4) - firstDay.getDate()) : ((newDate.getDate() + 4) - firstDay.getDate());
                    nextMonth = ((newDate.getMonth() + 1) <= 9 ? ("0" + (newDate.getMonth() + 2)) : (newDate.getMonth() + 2));
                    if (nextMonth == 13) {
                        nextMonth = '0' + 1;
                        nextYear = nextYear + 1;
                    }
                }

                //Select1.val(
                //    nextDay
                //    + "."
                //    + nextMonth
                //    + "."
                //    +
                //    nextYear
                //);
            },
            beforeShowDay: function (date) {
                if (date.getDay() == 5)
                    return [1];
                else
                    return [0];
            }
        });
    }

    $('.Teamleiter input').prop('disabled', true)
});