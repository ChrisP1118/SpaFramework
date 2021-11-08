<template>
  <form-page-template :page-title="pageTitle" :item="item">
    <client-fields :item="item" v-on:submit="onSubmit" v-on:cancel="onCancel">
      <template v-slot:save>Save Client</template>
    </client-fields>
  </form-page-template>
</template>

<script>
import axios from "axios";
import FormMixin from '../Mixins/FormMixin.vue';

export default {
  name: "ClientEdit",
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
      return this.item.name;
    },
    pageTitle: function () {
      return 'Client: ' + this.itemTitle;
    }
  },
  methods: {
    load: function () {
      let url = '/api/client/' + this.id;

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
      this.$router.push('/client');
    },
    onSubmit(evt) {
      let url = '/api/client/' + this.id;

      axios
        .put(url, this.item)
        .then(response => {
          this.item = response.data;

          this.processEditSuccessResponse(response, 'Client');

          this.$router.push('/client');
        })
        .catch(error => {
          this.processEditErrorResponse(error, 'Client');
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