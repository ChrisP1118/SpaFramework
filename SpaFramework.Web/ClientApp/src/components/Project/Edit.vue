<template>
  <form-page-template :page-title="pageTitle" :item="item">
    <project-fields :item="item" v-on:submit="onSubmit" v-on:cancel="onCancel">
      <template v-slot:save>Save Project</template>
    </project-fields>
  </form-page-template>
</template>

<script>
import axios from "axios";
import FormMixin from '../Mixins/FormMixin.vue';

export default {
  name: "ProjectEdit",
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
      return 'Project: ' + this.itemTitle;
    }
  },
  methods: {
    load: function () {
      let url = '/api/project/' + this.id;

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
      this.$router.push('/project');
    },
    onSubmit(evt) {
      let url = '/api/project/' + this.id;

      axios
        .put(url, this.item)
        .then(response => {
          this.item = response.data;

          this.processEditSuccessResponse(response, 'Project');

          this.$router.push('/project');
        })
        .catch(error => {
          this.processEditErrorResponse(error, 'Project');
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