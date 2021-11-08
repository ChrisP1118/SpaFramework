<template>
  <form-template @submit="onSubmit" @cancel="onCancel">
    <template v-slot:save><slot name="save"></slot></template>
    <template>
      <b-row class="mt-3">
        <b-col xs="12" sm="6" lg="3">
          <h4>Basic</h4>
          <hr />
          <text-control label="Name" required v-model="item.name" :concurrency-check="item.concurrencyCheck"></text-control>
          <text-control label="Abbreviation" required v-model="item.abbreviation" :concurrency-check="item.concurrencyCheck"></text-control>
        </b-col>
      </b-row>
    </template>
  </form-template>
</template>

<script>
import { mapState, mapGetters } from 'vuex'

export default {
  name: "ClientFields",
  props: [
    'item'
  ],
  data() {
    return {
    };
  },
  computed: {
  },
  methods: {
    onCancel(evt) {
      this.$emit('cancel');
    },
    onSubmit(evt) {
      this.$emit('submit');
    }
  }
};
</script>