<template>
  <div>
    <b-container fluid class="py-3">
      <b-row>
        <b-col>
          <page-title :title="pageTitle" />
          <hr />
          <slot></slot>
        </b-col>
      </b-row>
    </b-container>
  </div>
</template>

<script>
export default {
  name: 'ListPageTemplate',
  props: [ 
    'pageTitle'
  ]
}
</script>