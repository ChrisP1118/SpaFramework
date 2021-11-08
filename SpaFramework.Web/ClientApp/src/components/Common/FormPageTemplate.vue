<template>
  <div>
    <b-container fluid class="py-3">
      <b-row>
        <b-col>
          <page-loading v-if="!hideTitle && !item" />
          <page-title v-if="!hideTitle && item" :title="pageTitle" />
          <hr v-if="!hideTitle" />
          <div v-if="item" :class="hideTitle ? '' : 'mt-3'">
            <slot></slot>
          </div>
        </b-col>
      </b-row>
    </b-container>
  </div>
</template>

<script>
export default {
  name: 'FormPageTemplate',
  props: [ 
    'hideTitle',
    'item',
    'pageTitle'
  ]
}
</script>