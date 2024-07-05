<script setup>
import { ref, watch } from 'vue'

const props = defineProps({
    required: {
        required: true
    },
    modelValue: {
    },
    attName: {
    }
}
)

const value = ref(props.modelValue)

const emit = defineEmits(['update:modelValue']);

watch(() => value.value, (newValue, oldValue) => {
    emit('update:modelValue', newValue);
})

</script>
<template>
    <div style="display: flex; flex-wrap: nowrap; flex-direction: row; margin: 1em;">
        <p>{{ attName }}</p>
        <el-input v-model="value" placeholder="Please input" type="textarea" />
        <el-icon size="2em" :color="required && (modelValue == null || modelValue == '') ? '#F56C6C' : '#67C23A'">
            <SuccessFilled />
        </el-icon>
    </div>
</template>