<script>
export default {
  name: "FormMixin",
  props: [
  ],
  data() {
    return {
    }
  },
  computed: {
  },
  methods: {
    loadFieldsFromUrl: function () {
      Object.keys(this.$route.query).forEach(k => {
        this.item[k] = this.$route.query[k];
      });
    },
    isFormValid: function (formName) {
      if (!formName)
        formName = 'Default';

      // We run through each item, rather than just calling .every, to ensure each item gets checked and can update its styling accordingly
      let isValid = true;
      this.$children.forEach(childComponent => {
        if (childComponent.formName && childComponent.formName != formName)
          return;
        if (typeof(childComponent.isValid) === 'undefined')
          return;
        if (!childComponent.isValid) {
          let label = '(Unknown)';
          if (childComponent && childComponent.label)
            label = childComponent.label;
          
          console.log('Validation failed: ' + label);
          isValid = false;
        }
      });

      return isValid;

      //return this.$children.every(childComponent => { return (childComponent.formName && childComponent.formName != formName) || typeof(childComponent.isValid) === 'undefined' || childComponent.isValid; });
    },
    processEditSuccessResponse: function (response, entityType) {
      this.processSuccessResponse(response, entityType, 'saved', 'Saved');
    },
    processAddSuccessResponse: function (response, entityType) {
      this.processSuccessResponse(response, entityType, 'created', 'Created');
    },
    processSuccessResponse: function (response, entityType, verb1, verb2) {
      this.processSuccessResponseRaw(response, entityType + ' ' + verb2, 'Your changes to "' + this.itemTitle + '" were ' + verb1 + ' successfully.');
    },
    processSuccessResponseRaw: function (response, title, message) {
      this.showSuccessMessage(title, message);
    },
    showSuccessMessage: function (title, message) {
      let parent = this.$parent;

      while (parent && !parent.showToast)
        parent = parent.$parent;

      parent.showToast(message, {
        title: title,
        variant: 'success',
        autoHideDelay: 5000
      });
    },
    processEditErrorResponse: function (error, entityType) {
      this.processErrorResponse(error, entityType, 'save', 'Saving');
    },
    processAddErrorResponse: function (error, entityType) {
      this.processErrorResponse(error, entityType, 'create', 'Creating');
    },
    processGenericErrorResponse: function (title, error) {
      let errorMessage = 'An error occurred: ';

      if (error.response.status == 400)
        errorMessage = 'An error occurred.';
      else if (error.response.status == 403)
        errorMessage = 'You do not have access to perform this operation.';
      else if (error.response.status == 409)
        errorMessage = 'The item has been edited since you loaded the page. Reload and try again.';

      if (error && error.response && error.response.data && error.response.data.Details)
        errorMessage += ' ' + error.response.data.Details;

      this.showErrorMessage(title, errorMessage)
    },
    processErrorResponse: function (error, entityType, verb1, verb2) {
      console.log(error);

      let errorMessage = 'Unable to ' + verb1 + ' "' + this.itemTitle + '". An unknown error occurred.';
      let detailedMessage = '';

      if (error && error.response && error.response.data && error.response.data.Details)
        detailedMessage = error.response.data.Details;

      if (error.response.status == 400)
        errorMessage = 'Unable to ' + verb1 + ' "' + this.itemTitle + '". ' + (detailedMessage ? detailedMessage : 'The data you provided could not be validated.');
      else if (error.response.status == 403)
        errorMessage = 'Unable to ' + verb1 + ' "' + this.itemTitle + '". You are not logged in or do not have access.';
      else if (error.response.status == 409)
        errorMessage = 'Unable to ' + verb1 + ' "' + this.itemTitle + '". The item has been edited since you loaded the page. Reload this page to get the latest changes.';

      this.showErrorMessage('Error ' + verb2 + ' ' + entityType, errorMessage)
    },
    showErrorMessage: function (title, message) {
      let parent = this.$parent;

      while (parent && !parent.showToast)
        parent = parent.$parent;

      parent.showToast(message, {
        title: title,
        variant: 'danger',
        autoHideDelay: 5000
      });      
    }
  },
  mounted () {
  }
};
</script>