<template>
  <form-page-template :page-title="pageTitle" :item="item">
    <client-fields :item="item" v-on:submit="onSubmit" v-on:cancel="onCancel">
      <template v-slot:save>Add Client</template>
    </client-fields>
  </form-page-template>
</template>

<script>
import axios from "axios";
import FormMixin from '../Mixins/FormMixin.vue';

export default {
  name: "ClientAdd",
  mixins: [FormMixin],
  props: [],
  data() {
    return {
      item: {
        name: null
      }
    };
  },
  computed: {
    itemTitle: function () {
      if (this.item.name)
        return this.item.name;
      else
        return null;
    },
    pageTitle: function () {
      return 'New Client' + (this.itemTitle && this.itemTitle.length > 0 ? ': ' + this.itemTitle : '');
    }
  },
  methods: {
    load: function () {
      this.loadFieldsFromUrl();
    },
    onCancel(evt) {
      this.$router.push('/client');
    },
    onSubmit(evt) {
      let url = '/api/client';

      axios
        .post(url, this.item)
        .then(response => {
          this.item = response.data;

          this.processAddSuccessResponse(response, 'Client');

          this.$router.push('/client');
        })
        .catch(error => {
          this.processAddErrorResponse(error, 'Client');
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