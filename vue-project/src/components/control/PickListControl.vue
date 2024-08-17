<script setup>
import { ref, watch, onMounted } from 'vue'
import { daxHelper } from '../../daxHelper.js'

const props = defineProps({
    required: {
        required: true
    },
    disabled: {
        required: true
    },
    modelValue: {
        required: true
    },
    labelName: {
    },
    entityLogicName: {
        required: true
    },
    attributeType: {
        required: true
    },
    attributeName: {
        required: true
    }
}
)

const options = ref([])
const entityDefinition = ref({})
const attributeDefinition = ref({})
const isMultiple = ref(false)
const selectedItem = ref(null)

onMounted(() => {
    setOptions();
    setDefaultValue(props.modelValue);
})

const emit = defineEmits(['update:modelValue'])

watch(() => selectedItem.value, (newValue, oldValue) => {
    if (isMultiple.value) {
        if (newValue.length == 0) {
            emit('update:modelValue', null);
        }
        else {
            emit('update:modelValue', newValue.join(","));
        }
    }
    else {
        if (newValue || newValue == 0) {
            emit('update:modelValue', newValue);
        }
        else {
            emit('update:modelValue', null);
        }
    }
})
watch(() => props.modelValue, (newValue, oldValue) => {
    setDefaultValue(newValue)
})

function setOptions() {
    options.value = [];
    entityDefinition.value = daxHelper.getEntityDefinitionByLogicalName(props.entityLogicName);
    attributeDefinition.value = daxHelper.getAttributeDefinition(props.entityLogicName, props.attributeName);
    if (props.attributeType == 'Picklist') {
        setPicklistOptions();
    }
    else if (props.attributeType == "Boolean") {
        setBooleanOptions();
    }
    else if (props.attributeType == 'State') {
        setStateOptions();
    }
    else if (props.attributeType == 'Status') {
        setStatusOptions();
    }
    else if (props.attributeType == 'Virtual') {
        isMultiple.value = true;
        setMultiSelectOptions();
    }
    else {
        throw new Error('unsupported AttributeType: ' + props.attributeType);
    }
    daxHelper.attributesOptionsetMap.set(attributeDefinition.value.MetadataId, options.value);
}
function setDefaultValue(modelValue) {
    if (modelValue || modelValue == 0) {
        if (!isMultiple.value) {
            selectedItem.value = modelValue;
        }
        else {
            selectedItem.value = modelValue.split(",").map(Number);
        }
    }
}
function setPicklistOptions() {
    let attributeInfo = daxHelper.httpGetWebApiJsonResponse(`EntityDefinitions(${entityDefinition.value.MetadataId})/Attributes(${attributeDefinition.value.MetadataId})/Microsoft.Dynamics.CRM.PicklistAttributeMetadata?$expand=OptionSet,GlobalOptionSet`);
    let optionsInfos = attributeInfo.OptionSet.Options;
    optionsInfos.forEach(x => {
        options.value.push({ value: x.Value, label: x.Label.UserLocalizedLabel.Label });
    });
}
function setBooleanOptions() {
    let attributeInfo = daxHelper.httpGetWebApiJsonResponse(`EntityDefinitions(${entityDefinition.value.MetadataId})/Attributes(${attributeDefinition.value.MetadataId})/Microsoft.Dynamics.CRM.BooleanAttributeMetadata?$expand=OptionSet,GlobalOptionSet`);
    let optionSetInfos = attributeInfo.OptionSet;
    options.value.push({ value: false, label: optionSetInfos.FalseOption.Label.UserLocalizedLabel.Label });
    options.value.push({ value: true, label: optionSetInfos.TrueOption.Label.UserLocalizedLabel.Label });
}
function setStateOptions() {
    let attributeInfo = daxHelper.httpGetWebApiJsonResponse(`EntityDefinitions(${entityDefinition.value.MetadataId})/Attributes(${attributeDefinition.value.MetadataId})/Microsoft.Dynamics.CRM.StateAttributeMetadata?$expand=OptionSet,GlobalOptionSet`);
    let optionsInfos = attributeInfo.OptionSet.Options;
    optionsInfos.forEach(x => {
        options.value.push({ value: x.Value, label: x.Label.UserLocalizedLabel.Label });
    });
}

function setStatusOptions() {
    let attributeInfo = daxHelper.httpGetWebApiJsonResponse(`EntityDefinitions(${entityDefinition.value.MetadataId})/Attributes(${attributeDefinition.value.MetadataId})/Microsoft.Dynamics.CRM.StatusAttributeMetadata?$expand=OptionSet,GlobalOptionSet`);
    let optionsInfos = attributeInfo.OptionSet.Options;
    optionsInfos.forEach(x => {
        options.value.push({ value: x.Value, label: x.Label.UserLocalizedLabel.Label });
    });
}

function setMultiSelectOptions() {
    let attributeInfo = daxHelper.httpGetWebApiJsonResponse(`EntityDefinitions(${entityDefinition.value.MetadataId})/Attributes(${attributeDefinition.value.MetadataId})/Microsoft.Dynamics.CRM.MultiSelectPicklistAttributeMetadata?$expand=OptionSet,GlobalOptionSet`);
    let optionsInfos = attributeInfo.OptionSet.Options;
    optionsInfos.forEach(x => {
        options.value.push({ value: x.Value, label: x.Label.UserLocalizedLabel.Label });
    });
}
</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em; align-items: center;">
        <div class="controlLabel">
            <p>{{ labelName }}</p>
            <p v-if="attributeName">{{ attributeName }}</p>
        </div>

        <el-select v-if="!isMultiple" v-model="selectedItem" placeholder="Select" size="large" :disabled=disabled clearable>
            <el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value" />
        </el-select>

        <el-select v-if="isMultiple" v-model="selectedItem" placeholder="Select" size="large" :disabled=disabled multiple>
            <el-option v-for="item in options" :key="item.value" :label="item.label" :value="item.value" />
        </el-select>

        <el-icon class="requiredIcon" style="margin-left: 5px;" size="2em">
            <WarningFilled color="#F56C6C" v-if="required && selectedItem == null" />
            <SuccessFilled color="#67C23A" v-else />
        </el-icon>
    </div>
</template>