<script setup>
import { ref, watch, onMounted } from 'vue'
import { daxHelper } from '../../daxHelper.js'

const props = defineProps({
    required: {
        required: true
    },
    modelValue: {
    },
    disabled: {
        required: true
    },
    labelName: {
    },
    attributeName: {
    }
}
)

const value = ref(props.modelValue)

const emit = defineEmits(['update:modelValue']);

watch(() => value.value, (newValue, oldValue) => {
    if (newValue) {
        newValue = daxHelper.getCrmDateTimeString(newValue);
    }
    emit('update:modelValue', newValue);
})
watch(() => props.modelValue, (newValue, oldValue) => {
    value.value = newValue;
})
onMounted(() => {

})

</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em; align-items: center;">
        <div class="controlLabel">
            <p>{{ labelName }}</p>
            <p v-if="attributeName">{{ attributeName }}</p>
        </div>
        <el-date-picker class="controlItem" :disabled=disabled format="YYYY-MM-DDTHH:mm:ssZ" v-model="value" type="datetime"
            placeholder="Select date and time" />
        <el-tooltip placement="top" effect="light">
            <template #content>The time zone is the same as the time zone of the current pc.</template>
            <el-icon>
                <Warning />
            </el-icon>
        </el-tooltip>
        <el-icon class="requiredIcon" style="margin-left: 5px;" size="2em">
            <WarningFilled color="#F56C6C" v-if="required && (modelValue == null || modelValue == '')" />
            <SuccessFilled color="#67C23A" v-else />
        </el-icon>
    </div>
</template>