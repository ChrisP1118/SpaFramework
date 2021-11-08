<script>
import { DateTime } from 'luxon';

export default {
  name: "FormattingMixin",
  props: [
  ],
  data() {
    return {
    }
  },
  computed: {
  },
  methods: {
    getFormattedDate: function (d) {
      let dt = DateTime.fromISO(d);
      return dt.toLocaleString(DateTime.DATE_MED_WITH_WEEKDAY);
    },
    getFormattedDateTime: function (d) {
      let dt = DateTime.fromISO(d);
      return dt.toLocaleString(DateTime.DATE_MED_WITH_WEEKDAY) + ' ' + dt.toFormat('h:mm a ZZZZ');
    },
    getSelectText: function (value, selectOptions) {
      let opt = selectOptions.find(x => x.value == value);
      if (opt)
        return opt.text;
      else
        return opt;
    },
    getCamelSpacedText: function (value) {
      return value.replace(/([A-Z])/g, ' $1').trim()
    },
    getFormattedPhoneNumber: function (value) {
      if (value.length != 10)
        return value;

      return value.substring(0, 3) + '-' + value.substring(3, 6) + '-' + value.substring(6);
    }
  },
  mounted () {
  }
};
</script>