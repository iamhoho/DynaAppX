<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
    required: {
        required: true
    },
    modelValue: {
    },
    labelName: {
    },
    disabled: {
        required: true
    },
    attributeName: {
    }
}
)

const value = ref(props.modelValue)

const emit = defineEmits(['update:modelValue'])

watch(() => value.value, (newValue, oldValue) => {
    if (newValue) {
        emit('update:modelValue', newValue);
    }
    else {
        emit('update:modelValue', null);
    }
})
watch(() => props.modelValue, (newValue, oldValue) => {
    value.value = newValue;
})

</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em; align-items: center;">
        <div class="controlLabel">
            <p>{{ labelName }}</p>
            <p v-if="attributeName">{{ attributeName }}</p>
        </div>
        <el-input class="controlItemTextarea" v-model="value" placeholder="Please input" type="textarea"
            :disabled=disabled />
        <el-icon class="requiredIcon" style="margin-left: 5px;" size="2em">
            <WarningFilled color="#F56C6C" v-if="required && (modelValue == null || modelValue == '')" />
            <SuccessFilled color="#67C23A" v-else />
        </el-icon>
    </div>
</template>