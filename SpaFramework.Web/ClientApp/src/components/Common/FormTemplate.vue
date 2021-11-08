<template>
  <div>
    <b-form novalidate @submit.prevent="onSubmit">
      <slot></slot>
      <b-row class="mt-3" v-if="!hideButtons">
        <b-col>
          <b-button-toolbar key-nav>
            <b-button-group>
              <b-button type="submit" variant="primary">
                <slot name="save"></slot>
              </b-button>
            </b-button-group>
            <b-button-group class="mx-2" v-if="!hideCancel">
              <b-button type="button" @click="onCancel">Cancel</b-button>
            </b-button-group>
          </b-button-toolbar>
        </b-col>
      </b-row>
          
    </b-form>
  </div>
</template>

<script>
import FormMixin from '../Mixins/FormMixin.vue';

export default {
  name: 'FormTemplate',
  mixins: [FormMixin],
  props: [
    'hideButtons',
    'hideCancel'
  ],
  methods: {
    onCancel(evt) {
      this.$emit('cancel');
    },
    onSubmit(evt) {
      if (this.isFormValid())
        this.$emit('submit');
    }
  }

}
</script>