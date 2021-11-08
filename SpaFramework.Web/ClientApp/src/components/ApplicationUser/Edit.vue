<template>
  <form-page-template :page-title="pageTitle" :item="item">
    <application-user-fields :item="item" v-on:submit="onSubmit" v-on:cancel="onCancel">
      <template v-slot:save>Save User</template>
    </application-user-fields>
  </form-page-template>
</template>

<script>
import axios from "axios";
import FormMixin from '../Mixins/FormMixin.vue';

export default {
  name: "ApplicationUserEdit",
  mixins: [FormMixin],
  props: ['id'],
  data() {
    return {
      item: null
    };
  },
  computed: {
    itemTitle: function () {
      if (!this.item)
        return null;
      return this.item.userName;
    },
    pageTitle: function () {
      return 'User: ' + this.itemTitle;
    }
  },
  methods: {
    load: function () {
      let url = '/api/applicationUser/' + this.id + "?includes=outlets,roles";

      axios
        .get(url)
        .then(response => {
          this.item = response.data;
        })
        .catch(error => {
          console.log(error);
        });
    },
    onCancel(evt) {
      this.$router.push('/applicationUser');
    },
    onSubmit(evt) {
      let url = '/api/applicationUser/' + this.id;

      axios
        .put(url, this.item)
        .then(response => {
          this.item = response.data;

          this.processEditSuccessResponse(response, 'User');

          this.$router.push('/applicationUser');
        })
        .catch(error => {
          this.processEditErrorResponse(error, 'User');
        });
    }    
  },
  mounted () {
    this.load();
    // Use this instead of the previous line to test the "Loading" bar
    //window.setTimeout(this.load, 3000);
  }
};
</script>