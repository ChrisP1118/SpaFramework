<template>
  <form-template @submit="onSubmit" @cancel="onCancel">
    <template v-slot:save><slot name="save"></slot></template>
    <template>
      <b-row class="mt-3">
        <b-col xs="12" sm="6" lg="3">
          <h4>Basic</h4>
          <hr />
          <text-control label="Name" required v-model="item.name" :concurrency-check="item.concurrencyCheck"></text-control>
          <select-list-control label="Client" required v-model="item.clientId" :options="clients.selectOptions" :concurrency-check="item.concurrencyCheck"></select-list-control>
        </b-col>
        <b-col xs="12" sm="6" lg="3">
          <h4>Status</h4>
          <hr />
          <select-list-control label="State" required v-model="item.state" :options="projectStateSelectOptions" :concurrency-check="item.concurrencyCheck"></select-list-control>
          <date-picker-control label="Start Date" required v-model="item.startDate" :date-disabled-function="() => {return false;}" :concurrency-check="item.concurrencyCheck"></date-picker-control>
          <date-picker-control label="End Date" required v-model="item.endDate" :date-disabled-function="() => {return false;}" :concurrency-check="item.concurrencyCheck"></date-picker-control>
        </b-col>
      </b-row>
    </template>
  </form-template>
</template>

<script>
import { mapState, mapGetters } from 'vuex'

export default {
  name: "ProjectFields",
  props: [
    'item'
  ],
  data() {
    return {
    };
  },
  computed: {
    ...mapState('cachedData', ['projectStateSelectOptions', 'clients']),
  },
  methods: {
    load() {
      this.$store.dispatch('cachedData/loadClients');
    },
    onCancel(evt) {
      this.$emit('cancel');
    },
    onSubmit(evt) {
      this.$emit('submit');
    }
  },
  mounted() {
    this.load();
  }
};
</script>